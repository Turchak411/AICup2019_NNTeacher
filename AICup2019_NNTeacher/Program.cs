using System;
using System.Diagnostics;

namespace AICup2019_NNTeacher
{
    class Program
    {
        private static FileManager _fileManager;

        static void Main(string[] args)
        {
            int trainStartCount = 13821;
            int trainEndCount = 16420;

            // Для блочного обучения указать:
            int startDataSetIndex = 0;
            int endDataSetIndex = 100000;

            #region Set process settings

            Process thisProc = Process.GetCurrentProcess();
            thisProc.PriorityClass = ProcessPriorityClass.High;

            #endregion

            const int receptors = 75;

            const int numberOfOutputClasses = 1; // Количество наших классов
            int[] neuronByLayer = { 50, 50, numberOfOutputClasses };

            _fileManager = new FileManager();

            var networkTeacher = new NetworkTeacher(neuronByLayer, receptors, 13, _fileManager)
            {
                Iteration = trainEndCount,
                TestVectors = _fileManager.ReadVectors("inputDataTestPart_temp.txt")
            };

            networkTeacher.TrainNet(trainStartCount);

            networkTeacher.PrintLearnStatistic(true);

            Console.ReadKey();
        }
    }
}
