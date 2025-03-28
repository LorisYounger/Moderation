using System;
using System.Collections.Concurrent;
using System.IO;
using ChatGPT.API.Framework;
using Microsoft.ML;
using Microsoft.ML.Data;
using Moderation.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static ChatGPT.API.Framework.Moderations;

namespace PreTrain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("欢迎使用 Moderation 预处理程序");
            Console.WriteLine("1. 为txt纯文本数据使用AI进行标签");
            Console.WriteLine("2. 使用数据集进行预训练");
            switch (Console.ReadLine())
            {
                case "1":
                    AITag.StartTag();
                    break;              
                default:
                    break;
            }

        }

    }
}
