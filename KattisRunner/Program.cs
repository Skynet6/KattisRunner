using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace KattisRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length<=0)
            {
                Console.WriteLine("Please specify args. You have two options:");
                Console.WriteLine("----------------------------");
                Console.WriteLine("\t-Program path");
                Console.WriteLine("\t-Input file path");
                Console.WriteLine("\t-Output file path");
                Console.WriteLine("----------------------------");
                Console.WriteLine("\t-Program path");
                Console.WriteLine("\t-Directory with files");
                Console.WriteLine("----------------------------");
                return;
            }
            
            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"Path '{args[0]}' is not valid path to file");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(args[0]);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;

            foreach (Sample data in GetSamples(args))
            {
                Console.WriteLine($"Running {data.Name}");
                Stopwatch sw = Stopwatch.StartNew();
                Run(startInfo, data.Input, data.Output);
                Console.WriteLine($"Elapsed {sw.ElapsedMilliseconds}ms");
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static IEnumerable<Sample> GetSamples(string[] args)
        {
            if (args.Length == 2)
            {   // passed directory
                foreach (string inputFile in Directory.EnumerateFiles(args[1], "*.in"))
                {
                    string outputFile = inputFile.Replace(".in", ".out");
                    if (File.Exists(outputFile))
                    {
                        yield return new Sample(Path.GetFileNameWithoutExtension(inputFile), inputFile, outputFile);
                    }
                }

            }

            if (args.Length == 3)
            {   // passed two files
                yield return new Sample(Path.GetFileNameWithoutExtension(args[1]), args[1], args[2]);
            }
        }

        private static void Run(ProcessStartInfo startInfo, string inputFile, string outputFile)
        {
            using (Process process = Process.Start(startInfo))
            {
                Task outputTask = Task.Factory.StartNew(() =>
                {
                    using (StreamReader reader = process.StandardOutput)
                    using (FileStream fs = new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (StreamReader output = new StreamReader(fs))
                    {
                        int i = 0;
                        while (!reader.EndOfStream)
                        {
                            string result = reader.ReadLine();
                            string expected = output.ReadLine();
                            if (result == expected)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Case {i + 1} OK");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Case {i + 1} Failed\n\t({result})\n\t({expected})");
                            }
                            i++;
                            Console.ResetColor();
                        }
                        if (i == 0)
                        {
                            Console.WriteLine("No read output");
                        }
                    }
                });
                Task inputTask = Task.Factory.StartNew(() =>
                {
                    using (StreamWriter writer = process.StandardInput)
                    using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (StreamReader input = new StreamReader(fs))
                    {
                        while (!input.EndOfStream)
                        {
                            writer.WriteLine(input.ReadLine());
                        }
                    }
                });

                Task.WaitAll(inputTask, outputTask);
            }
        }
    }
}
