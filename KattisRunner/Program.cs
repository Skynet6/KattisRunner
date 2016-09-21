using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace KattisRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(args[0]);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;

            var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Process is null");
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            var outputTask = Task.Factory.StartNew(() =>
            {
                using (StreamReader reader = process.StandardOutput)
                using (FileStream fs = new FileStream(args[2], FileMode.Open, FileAccess.Read))
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
                    if(i==0)
                    {
                        Console.WriteLine("No read output");
                    }
                }
            });
            var inputTask = Task.Factory.StartNew(() => {
                using (StreamWriter writer = process.StandardInput)
                using (FileStream fs = new FileStream(args[1], FileMode.Open, FileAccess.Read))
                using (StreamReader input = new StreamReader(fs))
                {
                    while (!input.EndOfStream)
                    {
                        writer.WriteLine(input.ReadLine());
                    }
                }
            });
            Task.WaitAll(inputTask, outputTask);
            Console.WriteLine($"Elapsed {sw.ElapsedMilliseconds}ms");

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
