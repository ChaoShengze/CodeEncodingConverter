using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MultithreadingScaffold;

namespace CodeEncodingConverter
{
    internal class Program
    {
        /// <summary>
        /// CodePage Dict.
        /// </summary>
        private static Dictionary<string, Encoding> CodePages = new();

        public static void Main()
        {
            try
            {
                InitCodePagesDict();

                WriteLine("Please input converting source path.");
                var source = Console.ReadLine();
                WriteLine("Please input type you want to convert, like .cs,.cpp,.txt");
                var targets = Console.ReadLine().Split(',');
                WriteLine("Please choose code page source used.");
                WriteLine("1. utf8");
                WriteLine("2. gb2312");
                var sourceCodepage = Console.ReadLine();
                WriteLine("Please choose code page want use.");
                WriteLine("1. utf8");
                WriteLine("2. gb2312");
                var targetCodepage = Console.ReadLine();

                HandleFiles(source, targets, CodePages[sourceCodepage], CodePages[targetCodepage]);
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Write info to Console.
        /// </summary>
        /// <param name="line"></param>
        private static void WriteLine(string line)
        {
            var datetime = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"> {datetime}：{line}");
        }
        /// <summary>
        /// Init CodePages Dict.
        /// </summary>
        private static void InitCodePagesDict()
        {
            CodePages.Add("1", Encoding.UTF8);
            CodePages.Add("2", CodePagesEncodingProvider.Instance.GetEncoding("gb2312"));
        }
        /// <summary>
        /// Start coverting files into selected code pages.
        /// </summary>
        /// <param name="startPath">Files root path.</param>
        /// <param name="targets">File types you want to convert.</param>
        /// <param name="targetEncoding">Encoding settings.</param>
        private static void HandleFiles(string startPath, string[] targets, Encoding sourceEncoding, Encoding targetEncoding)
        {
            if (!Directory.Exists(startPath))
            {
                WriteLine("Illegal source path!");
                return;
            }

            if (targets == null || targets.Length == 0)
            {
                WriteLine("Illegal file type!");
                return;
            }

            if (targetEncoding == null)
            {
                WriteLine("Illegal encoding setting!");
                return;
            }

            var files = Directory.GetFiles(startPath, "*", SearchOption.AllDirectories);
            var _files = new List<string>();

            foreach (var file in files)
                foreach (var type in targets)
                    if (Path.GetExtension(file) == type)
                        _files.Add(file);

            //Using MultithreadingScaffold start multithreading task.
            MTScaffold mTScaffold = new MTScaffold()
            {
                Workload = _files.Count,
                Final = () =>
                {
                    WriteLine("All task complete!");
                },
                Worker = (i) =>
                {
                    var file = _files[i];
                    foreach (var type in targets)
                    {
                        WriteLine($"Handling file：{file}");
                        var text = File.ReadAllLines(file, sourceEncoding);
                        File.WriteAllLines(file, text, targetEncoding);
                    }
                },
                InNewThread = false,
                WriteConsole = true
            };
            mTScaffold.Start();
        }
    }
}
