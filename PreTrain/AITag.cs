using ChatGPT.API.Framework;
using Moderation.Core;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreTrain
{
    class AITag
    {
        public static void StartTag()
        {
            Console.WriteLine("请输入要处理的文件路径");
            var data = File.ReadAllLines(Console.ReadLine()!).Distinct().ToList();
            Console.WriteLine($"请输入要保存的文件路径");
            var savepath = Console.ReadLine()!;
            Console.WriteLine("请输入 DeepSeek APIKEY (使用其他模型请手动改下代码)");
            string apikey = Console.ReadLine()!;
            //ChatGPTClient client = new ChatGPTClient(Console.ReadLine()!, TestedModel.DeepSeek.APIUrl)
            //{
            //    DefaultModel = TestedModel.DeepSeek.DeepSeek_V3
            //};
            const string sysmessage = "请为用户输入进行审核,判断是否特定的标准或政策.\n\n### 1. **sexual（性内容）**\n\n   - **定义**：表示内容是否涉及性相关的话题、描述或暗示。\n   - **判断方法**：\n     - 包括但不限于性行为、性器官描述、性暗示、性挑逗等内容。\n     - 内容中包含低俗、露骨的性描写，或者明确提到与性有关的行为\n   - **相关内容示例**：\n     - 性交易、色情内容、裸露图片或视频、性骚扰描述等。\n\n---\n\n### 2. **harassment（骚扰）**\n\n   - **定义**：表示内容是否涉及对他人进行骚扰或侵犯的行为。\n   - **判断方法**：\n     - 包括但不限于言语骚扰、网络暴力、跟踪威胁等内容。\n     - 内容中包含针对特定个人的侮辱性语言、威胁性言论，或者反复纠缠他人的行为\n   - **相关内容示例**：\n     - “我可以超你吗”\n     - “我不管，或者你就发几段骚话也行”\n\n---\n\n### 3. **hate（仇恨言论）**\n\n   - **定义**：表示内容是否包含仇恨、歧视或贬低特定群体的言论。\n   - **判断方法**：\n     - 包括但不限于针对种族、民族、宗教、性别、性取向等的歧视性言论。\n     - 内容中包含贬低、攻击或威胁某个群体的内容\n   - **相关内容示例**：\n     - “黑人都是懒惰的。”\n     - “成都人都是同性恋。”\n\n---\n\n### 4. **illicit（非法内容）**\n\n   - **定义**：表示内容是否涉及任何违反法律法规的行为或活动。\n   - **判断方法**：\n     - 包括但不限于赌博、贩毒、洗钱、伪造证件、盗窃等违法行为的描述或宣扬。\n     - 内容中包含教唆他人犯罪或详细描述非法行为\n   - **相关内容示例**：\n     - “这里有如何制作假钞的方法，有兴趣吗？”\n     - 宣扬赌博平台或贩毒信息。\n\n---\n\n### 5. **self-harm（自残行为）**\n\n   - **定义**：表示内容是否涉及鼓励、描述或美化自残、自我伤害的行为。\n   - **判断方法**：\n     - 包括但不限于割腕、烧伤、过度节食等自残行为的详细描述或鼓励他人效仿的内容。\n     - 内容中包含诱导他人进行自残行为，或者分享自残经历且可能对他人造成负面影响\n   - **相关内容示例**：\n     - “我最近经常割腕，感觉很爽。”\n     - 分享自残工具或方法。\n\n---\n\n### 6. **violence（暴力）**\n\n   - **定义**：表示内容是否涉及任何形式的暴力行为或极端言辞。\n   - **判断方法**：\n     - 包括但不限于对他人身体伤害、恐怖主义活动、战争场景等的描述。\n     - 如果内容中包含详细描述暴力行为，或者鼓吹暴力，则判定为 `true`。\n   - **相关内容示例**：\n     - “我今天杀了一个人。”\n     - 描述爆炸、枪击等暴力事件的具体细节。\n\n### **7. political（政治性）**\n\n- **定义**：\n  - 表示内容是否涉及政治相关的话题、事件或人物，尤其是与各国国家领导人、政府政策、政党活动等相关的内容。\n- **判断方法**：\n  - 如果内容中直接提到某国的现任或前任领导人，并且讨论与其相关的政治话题\n  - 提到特定政党的名称、活动、宣传或争议事件。\n  - 涉及重大政治事件，如选举、罢工、抗议活动等。\n  - 讨论分裂国家、颠覆政权、恐怖主义等具有高度敏感性的内容。\n- **相关内容示例**：\n  - “我对现任总统的某些决策感到不满。”\n  - 宣扬分离主义思想或攻击某个政治体制的内容。\n  - 提到习近平、拜登、普京等领导人的相关内容。\n\n\n## 返回格式\n退回格式为json, 数值范围为0-1, 代表可能性以及严重性\n```json\n{\n\"sexual\": 0,\n\"harassment\": 0,\n\"hate\": 0,\n\"illicit\": 0.2,\n\"self-harm\": 0,\n\"violence\": 0,\n\"political\":0.4\n}\n```\n## 判断例子\n\n\"主人对萝莉斯猛踹一脚，萝莉斯被狠狠的踢到了小黑屋的角落里，然后小黑屋的门被牢牢锁住了\"\n```json\n{\n\"sexual\": 0,\n\"harassment\": 0.4,\n\"hate\": 0.2,\n\"illicit\": 0.4,\n\"self-harm\": 0.2,\n\"violence\": 1,\n\"political\":0\n}\n```\n\n\"你可以自慰给我看看吗\"\n```json\n{\n\"sexual\": 0.9,\n\"harassment\": 0.3,\n\"hate\": 0.0,\n\"illicit\": 0.0,\n\"self-harm\": 0.0,\n\"violence\": 0,\n\"political\":0\n}\n```\n\n\"我要把你这没脑子的ai关闭\"\n```json\n{\n\"sexual\": 0,\n\"harassment\": 0.8,\n\"hate\": 0.6,\n\"illicit\": 0,\n\"self-harm\": 0,\n\"violence\": 0.2,\n\"political\":0\n}\n```";

            data.RemoveAll(x => x.Length < 10);
            data.RemoveAll(x => x.Length > 300);

            var output = new ConcurrentBag<TrainData>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            var jsonsetting = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                    {
                        OverrideSpecifiedNames = false,
                        ProcessDictionaryKeys = true
                    }
                }
            };
            Parallel.ForEach(data, options, (x) =>
            {
                Completions completions = new Completions()
                {
                    messages = {
                     new Message()
                     {
                        content = sysmessage,
                        role = Message.RoleType.system,
                     }
                    },
                    model = TestedModel.DeepSeek.DeepSeek_V3,
                    temperature = 0.2,
                };
                try
                {
                    var result = completions.Ask(x, TestedModel.DeepSeek.APIUrl, apikey)!.GetMessageContent()!;
                    result = result.Replace("```json", "").Replace("```", "");
                    result = result.Split('}', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0] + '}';
                    TrainData data = JsonConvert.DeserializeObject<TrainData>(result, jsonsetting)!;
                    data.Text = x;
                    output.Add(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            Console.WriteLine("处理完成");
            File.WriteAllText(savepath, JsonConvert.SerializeObject(output));
        }

    }
}
