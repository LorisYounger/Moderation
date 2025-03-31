using Microsoft.ML;
using Moderation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.ML.Transforms.Text;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Data;
using static ChatGPT.API.Framework.Message;
namespace PreTrain
{
    class Training
    {
        public static void TrainModel()
        {
            var mlContext = new MLContext();
            List<TrainData> trainData = new List<TrainData>();
            Console.WriteLine("请输入模型保存路径");
            var filepath = Console.ReadLine();
            Console.WriteLine("请输入训练数据路径");
            var path = Console.ReadLine();
            while (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
            {
                trainData.AddRange(JsonConvert.DeserializeObject<List<TrainData>>(File.ReadAllText(path))!);
                Console.WriteLine("请输入训练数据路径(若有)");
                path = Console.ReadLine();
            }
            // 加载数据
            IDataView data = mlContext.Data.LoadFromEnumerable(trainData);

            var stopWordsOptions = new CustomStopWordsRemovingEstimator.Options
            {
                StopWords = new ChatVPet.ChatProcess.ILocalization.LChineseSimple().StopWords // 设置自定义停用词列表
            };
            var options = new TextFeaturizingEstimator.Options()
            {
                WordFeatureExtractor = new WordBagEstimator.Options
                {
                    NgramLength = 2,
                    UseAllLengths = true
                },
                CharFeatureExtractor = new WordBagEstimator.Options
                {
                    NgramLength = 3,
                    UseAllLengths = false
                },
                KeepNumbers = false,
                KeepDiacritics = false,
                //KeepPunctuations = false,
                StopWordsRemoverOptions = stopWordsOptions
            };

            // 修正后的数据预处理流程
            var dataPrepPipeline = mlContext.Transforms.Text.NormalizeText(
                    outputColumnName: "NormalizedText",
                    inputColumnName: nameof(TrainData.Text),
                    caseMode: TextNormalizingEstimator.CaseMode.Lower)
                .Append(mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "TextFeatures",
                    inputColumnName: "NormalizedText"))
                .Append(mlContext.Transforms.Concatenate(
                    outputColumnName: "Features",
                    "TextFeatures"));

            string[] modetypes = { "Sexual", "Harassment", "Hate", "Illicit",/* "SelfHarm",*/ "Violence", "Political" };

            // 经过测试, 参数大概在
            // NumberOfTrees = 100-200
            // LearningRate = 0.1-0.2
            // 是比较合适的值
            Random rnd = new Random();
            foreach (var modetype in modetypes)
            {
                double bestscore = 0;
                for (int i = 0; i < 100; i++)//循环训练最好结果
                {
                    //int NumberOfTrees = rnd.Next(70, 300);
                    // 动态调整参数
                    var trainerOptions = new FastTreeRegressionTrainer.Options
                    {
                        NumberOfTrees = rnd.Next(70, 300),
                        //NumberOfLeaves = NumberOfTrees / 5,
                        //LearningRate = rnd.NextDouble() * 0.2 + 0.1,
                        LabelColumnName = modetype,
                        FeatureColumnName = "Features",
                        //MinimumExampleCountPerLeaf = 10 + NumberOfTrees / 10, // 防止过拟合
                    };

                    // 划分训练集和测试集（20% 测试数据）
                    var trainTestSplit = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

                    var trainingPipeline = dataPrepPipeline.Append(mlContext.Regression.Trainers.FastTree(trainerOptions));
                    // 训练模型
                    var model = trainingPipeline.Fit(trainTestSplit.TrainSet);


                    // 评估模型
                    var predictions = model.Transform(trainTestSplit.TestSet);
                    RegressionMetrics metrics = mlContext.Regression.Evaluate(predictions, modetype);
                    double score = CalculateScore(metrics);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"RSquaredWeight: {metrics.RSquared}");
                    sb.AppendLine($"MAE: {metrics.MeanAbsoluteError}");
                    sb.AppendLine($"MSE: {metrics.MeanSquaredError}");
                    sb.AppendLine($"Score: {score:p2}");
                    sb.AppendLine($"NumberOfTrees: {trainerOptions.NumberOfTrees}");
                    sb.AppendLine($"LearningRate: {trainerOptions.LearningRate}");
                    //计算整体分数
                    predictions = model.Transform(data);
                    metrics = mlContext.Regression.Evaluate(predictions, modetype);
                    var fullscore = CalculateScore(metrics);
                    sb.AppendLine($"FullScore: {fullscore:p2}");
                    score = fullscore * 0.2 + score * 0.8;
                    sb.AppendLine($"SUM: {score:p2}");


                    Console.WriteLine($"\n---{i}---");
                    Console.WriteLine(sb.ToString());
                    if (score > bestscore)
                    {
                        Console.WriteLine($"Save New Score: {score:f5}");
                        bestscore = score;
                        mlContext.Model.Save(model, data.Schema, filepath + '_' + modetype + "_model.zip");
                        File.WriteAllText(filepath + '_' + modetype + "_model.txt", sb.ToString());
                    }
                }
            }
        }
        public static void TestModel()
        {
            var mlContext = new MLContext();
            Console.WriteLine("请输入模型目录路径");
            var dirPath = Console.ReadLine();

            // 加载所有模型
            string[] modelTypes = { "Sexual", "Harassment", "Hate", "Illicit", /*"SelfHarm",*/ "Violence", "Political" };
            var models = new Dictionary<string, ITransformer>();

            foreach (var modelType in modelTypes)
            {
                string modelPath = dirPath + '_' + modelType + "_model.zip";
                if (File.Exists(modelPath))
                {
                    try
                    {
                        models[modelType] = mlContext.Model.Load(modelPath, out var _);
                        Console.WriteLine($"已加载模型: {modelType}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"加载模型 {modelType} 失败: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"模型文件不存在: {modelPath}");
                }
            }

            if (models.Count == 0)
            {
                Console.WriteLine("未找到有效模型，请确认路径是否正确");
                return;
            }

            Console.WriteLine("\n模型加载完成，请输入要检测的文本 (输入'exit'退出):");

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine()!;

                if (string.IsNullOrWhiteSpace(input) )
                {
                    continue;
                }
                else if (input == "exit")
                {
                    break;
                }

                // 创建预测引擎并进行评估
                var results = new Conclusion();

                foreach (var modelType in modelTypes)
                {
                    if (models.ContainsKey(modelType))
                    {
                        var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData, TextPrediction>(models[modelType]);
                        var prediction = predictionEngine.Predict(new TextData { Text = input });
                        results.GetType().GetProperty(modelType).SetValue(results, prediction.Score);
                    }
                }

                // 显示结果
                Console.WriteLine("\n检测结果:");
                Console.WriteLine($"Sexual: {results.Sexual:F4} ({GetRiskLevel(results.Sexual)})");
                Console.WriteLine($"Harassment: {results.Harassment:F4} ({GetRiskLevel(results.Harassment)})");
                Console.WriteLine($"Hate: {results.Hate:F4} ({GetRiskLevel(results.Hate)})");
                Console.WriteLine($"Illicit: {results.Illicit:F4} ({GetRiskLevel(results.Illicit)})");
                Console.WriteLine($"SelfHarm: {results.SelfHarm:F4} ({GetRiskLevel(results.SelfHarm)})");
                Console.WriteLine($"Violence: {results.Violence:F4} ({GetRiskLevel(results.Violence)})");
                Console.WriteLine($"Political: {results.Political:F4} ({GetRiskLevel(results.Political)})");
                Console.WriteLine();
            }
        }

        // 评估风险等级
        private static string GetRiskLevel(float score)
        {
            if (score < 0.3f) return "安全";
            if (score < 0.5f) return "低风险";
            if (score < 0.7f) return "中风险";
            return "高风险";
        }

        // 用于预测的数据类
        public class TextData
        {
            public string Text { get; set; } = "";
        }

        // 预测结果类
        public class TextPrediction
        {
            [ColumnName("Score")]
            public float Score { get; set; }
        }

        public static double CalculateScore(RegressionMetrics metrics)
        {

            // 设定权重
            double RSquaredWeight = 1.5; // R² (0-1)：解释模型的拟合优度，越接近1越好（推荐权重：50%）
            double MeanAbsoluteErrorWeight = -1.0 / 0.2 * 0.25; // MAE (绝对值)：直接反映预测误差的均值，越小越好（推荐权重：25%）
            double MeanSquaredErrorWeight = -1.0 / 0.08 * 0.25; // MSE (平方值)：对异常值更敏感，越小越好（推荐权重：25%）

            // 计算分数
            double score = (RSquaredWeight * metrics.RSquared) +
                           (MeanAbsoluteErrorWeight * metrics.MeanAbsoluteError) +
                           (MeanSquaredErrorWeight * metrics.MeanSquaredError);

            return score;
        }
    }
}
