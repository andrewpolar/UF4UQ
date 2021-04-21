//This is DEMO for uncertainty quantification
//written by Dr. Andrew Polar and Dr. Mike Poluektov

using System;
using System.Collections.Generic;

namespace UQ
{
    class Program
    { 
        static void Main(string[] args)
        {
            //Build data
            int TrainingSize = 10000;
            double DataError = 0.6;
            DataHolder dhTraining = new DataHolder();
            dhTraining.BuildFormulaData(DataError, TrainingSize);

            int Number_of_trees = 16;  //must be either 2,4,8,16
            Forest forest = new Forest(dhTraining);
            forest.BuildForest(Number_of_trees);

            //Test accuracy
            int KSTRejected = 0;
            List<double> meanForEachMonteCarlo = new List<double>();
            List<double> meanForEachForest = new List<double>();
            List<double> singleOutput = new List<double>();
            List<double> singleModelOutput = new List<double>();
            int N = 100;
            for (int i = 0; i < N; ++i)
            {
                double[] randomInput = dhTraining.GetRandomInput();
                double[] MonteCarloOutput = dhTraining.GetStatData(randomInput, 200);

                singleOutput.Add(MonteCarloOutput[0]);
                Array.Sort(MonteCarloOutput);

                meanForEachMonteCarlo.Add(Static.GetMean(MonteCarloOutput));
                singleModelOutput.Add(forest.GetRootScalar(randomInput));

                double[] forestVotes = forest.GetSortedVotes(randomInput);

                meanForEachForest.Add(Static.GetMean(forestVotes));
                
                if (true == Static.KSTRejected005(MonteCarloOutput, forestVotes))
                {
                    ++KSTRejected;
                }
            }
            Console.WriteLine("Pearson for single tree and output = {0:0.0000}", Static.PearsonCorrelation(singleModelOutput.ToArray(), singleOutput.ToArray()));
            Console.WriteLine("Pearson for forest expectation  and output = {0:0.0000}", Static.PearsonCorrelation(meanForEachForest.ToArray(), singleOutput.ToArray()));
            Console.WriteLine("Pearson for forest expectation and Monte-Carlo expectation {0:0.0000}", Static.PearsonCorrelation(meanForEachMonteCarlo.ToArray(), meanForEachForest.ToArray()));
            Console.WriteLine("Kolmogorov-Smirnov rejected data sets {0} out of {1}", KSTRejected, N);
        }
    }
}
