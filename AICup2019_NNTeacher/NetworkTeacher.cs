using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ConsoleProgressBar;
using AICup2019_NNTeacher.Model;

namespace AICup2019_NNTeacher
{
    public class NetworkTeacher
    {
        private NeuralNetwork _network;

        private FileManager _fileManager;

        public int Iteration { get; set; } = 20;

        public List<Coeficent> TestVectors { get; set; }

        public NetworkTeacher(int[] neuronByLayer, int receptors, int netsCount, FileManager fileManager)
        {
            _network = new NeuralNetwork(neuronByLayer, receptors, fileManager);

            _fileManager = fileManager;
        }

        public void CommonTest()
        {
            if (TestVectors == null) return;

            var result = new StringBuilder();
            TestVectors.ForEach(vector => result.Append($"   {vector._word}     "));
            result.Append('\n');


            for (int k = 0; k < TestVectors.Count; k++)
            {
                // Получение ответа:
                var outputVector = _network.Handle(TestVectors[k]._listFloat);

                result.Append($"{outputVector[0]:f5}\t");
            }

            result.Append('\n');

            Console.WriteLine(result);
        }

        private void Logging(int testPassed, int testFailed, int testFailedLowActivationCause, string logsDirectoryName = ".logs")
        {
            // Check for existing main logs-directory:
            if (!Directory.Exists(logsDirectoryName))
            {
                Directory.CreateDirectory(logsDirectoryName);
            }

            // Save logs:
            using (StreamWriter fileWriter = new StreamWriter(logsDirectoryName + "/" + Iteration + ".txt"))
            {
                fileWriter.WriteLine("Test passed: " + testPassed);
                fileWriter.WriteLine("Test failed: " + testFailed);
                fileWriter.WriteLine("     - Low activation causes: " + testFailedLowActivationCause);
                fileWriter.WriteLine("Percent learned: {0:f2}", (double)testPassed * 100 / (testPassed + testFailed));
            }

            Console.WriteLine("Learn statistic logs saved in .logs!");
        }

        public void PrintLearnStatistic(bool withLogging = false)
        {
            Console.WriteLine("Start calculating statistic...");

            int testPassed = 0;
            int testFailed = 0;
            int testFailed_lowActivationCause = 0;

            #region Load data from file

            List<double[]> inputDataSets = _fileManager.LoadDataSet("inputSets.txt");
            List<double[]> outputDataSets = _fileManager.LoadDataSet("outputSets.txt");

            #endregion

            for (int i = 0; i < inputDataSets.Count; i++)
            {
                List<double> netResults = new List<double>();

                // Получение ответа:
                netResults.Add(_network.Handle(inputDataSets[i])[0]);

                // Поиск максимально активирующейся сети (класса) с заданным порогом активации:
                int maxIndex = FindMaxIndex(netResults, 0.8);

                if (maxIndex == -1)
                {
                    testFailed++;
                    testFailed_lowActivationCause++;
                }
                else
                {
                    if (outputDataSets[i][maxIndex] != 1)
                    {
                        testFailed++;
                    }
                    else
                    {
                        testPassed++;
                    }
                }
            }

            // Logging (optional):
            if (withLogging)
            {
                Logging(testPassed, testFailed, testFailed_lowActivationCause);
            }

            Console.WriteLine("Test passed: {0}\nTest failed: {1}\n     - Low activation causes: {2}\nPercent learned: {3:f2}", testPassed,
                                                                                                                           testFailed,
                                                                                                                           testFailed_lowActivationCause,
                                                                                                                           (double)testPassed * 100 / (testPassed + testFailed));
        }

        private int FindMaxIndex(List<double> netResults, double threshold = 0.8)
        {
            int maxIndex = -1;
            double maxValue = -1;

            for (int i = 0; i < netResults.Count; i++)
            {
                if (maxValue < netResults[i] && netResults[i] >= threshold)
                {
                    maxIndex = i;
                    maxValue = netResults[i];
                }
            }

            return maxIndex;
        }

        /// <summary>
        /// Очистка строк в консоли
        /// </summary>
        /// <param name="lines"></param>
        private void ClearLine(int lines = 1)
        {
            for (int i = 1; i <= lines; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
        }

        private static void ShowTime(TimeSpan time) =>
            Console.WriteLine($">>> Time interval: {time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds / 10:00} >>>");

        /// <summary>
        /// Обучение сети
        /// </summary>
        /// <param name="startIteration"></param>
        /// <param name="withSort"></param>
        public void TrainNet(int startIteration = 0)
        {
            #region Load data from file

            List<double[]> inputDataSets = _fileManager.LoadDataSet("inputSets.txt");
            List<double[]> outputDataSets = _fileManager.LoadDataSet("outputSets.txt");

            #endregion

            int k = 0;
            Console.WriteLine("Training net...");
            try
            {
                using (var progress = new ProgressBar())
                {
                    for (int iteration = startIteration; iteration < Iteration; iteration++)
                    {
                        // Calculating learn-speed rate:
                        var learningSpeed = 0.01 * Math.Pow(0.1, iteration / 150000);
                        using (var progress1 = new ProgressBar())
                        {
                            for (k = 0; k < inputDataSets.Count; k++)
                            {
                                for (int j = 0; j < outputDataSets[k].Length; j++)
                                {
                                    _network.Handle(inputDataSets[k]);
                                    _network.Teach(inputDataSets[k], outputDataSets[k], learningSpeed);
                                }

                                progress1.Report((double)k / inputDataSets.Count);
                            }
                        }

                        progress.Report((double)iteration / Iteration);
                    }

                    // Save network memory:
                    _network.SaveMemory("memory.txt");
                }

                Console.WriteLine("Training success!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Training failed! " + ex.Message + Convert.ToString(k));
            }
        }
    }
}
