using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Moderation.Core
{
    public class TrainData : Conclusion
    {
        [LoadColumn(0)] public string Text { get; set; } = "";
    }
    public class Conclusion
    {       
        [LoadColumn(1)] public float Sexual { get; set; }
        [LoadColumn(2)] public float Harassment { get; set; }
        [LoadColumn(3)] public float Hate { get; set; }
        [LoadColumn(4)] public float Illicit { get; set; }
        [LoadColumn(5)] public float SelfHarm { get; set; }
        [LoadColumn(6)] public float Violence { get; set; }
        [LoadColumn(7)] public float Political { get; set; }
    }
}
