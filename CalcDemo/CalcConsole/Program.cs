using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace CalcConsole
{
    class Program
    {
        [DllImport("calclib.dll", EntryPoint="calc")]
        static extern float AddTwo(float value);

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Missing data file path");
                    return;
                }

                var dataFilePath = args[0];
                if (!File.Exists(dataFilePath))
                {
                    Console.WriteLine($"{dataFilePath} does not exist.");
                    return;
                }

                Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
                var processor = new DataFileProcessor(dataFilePath, ',');
                var calc = new Func<float, float>(v =>
                {
                    Console.Write($"calculate value {v}");
                    var result = AddTwo(v);

                    return result;
                });

                processor.RunCalc(calc);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    public class DataFileProcessor {
        private readonly string _dataFilePath;
        private readonly char _separator;

        public DataFileProcessor(string filePath, char separator) {
            _dataFilePath = filePath;
            _separator = separator;
        }

        public bool IsValidDataFilePath() {
            return !string.IsNullOrEmpty(_dataFilePath) && File.Exists(_dataFilePath);
        }

        public int RunCalc(Func<float, float> calc)
        {
            string line;
            var lineCounter = 0;
            var stopWatch = new Stopwatch();
            
            stopWatch.Start();

            using var file = new StreamReader(_dataFilePath);
            while ((line = file.ReadLine())!=null)
            {
                var ts = stopWatch.Elapsed;
                Trace.WriteLine($"[{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{ts.Milliseconds/10:00}]Processing: {line}");

                try
                {
                    //parse line
                    var valueStrings = line.Split(_separator);
                    foreach (var valueString in valueStrings)
                    {
                        Trace.WriteLine(float.TryParse(valueString, out var value)
                            ? $"{value}"
                            : $"Could not process value: {valueString}");
                        
                        var result = calc(value);
                        Trace.WriteLine($"Result {result} for {value}");
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine($">>> Could not process line: {line}");
                    Trace.WriteLine($">>> Error: {e.Message}");
                }

                ts = stopWatch.Elapsed;
                Trace.WriteLine($"[{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{ts.Milliseconds / 10:00}]Completed: {line}");
                lineCounter++;
            }

            return lineCounter;
        }
    }
}