using System;
using System.Collections.Generic;
using System.Text;

namespace UQ
{
    enum InitType
    {
        CONST, RANDOM, LINEAR, NONE
    }

	class Static
	{
        static Random _rnd = new Random();

        static public List<int[]> GetRandomSets(int dataSize, int groupSize, int nGroups)
        {
            int nDataSets = groupSize * nGroups / dataSize;
            if (nDataSets * dataSize < groupSize * nGroups) nDataSets += 1;

            int[] mergedData = new int[nDataSets * dataSize];
            int counter = 0;
            for (int i = 0; i < nDataSets; ++i)
            {
                int[] position = Shuffle(dataSize);
                for (int j = 0; j < position.Length; ++j)
                {
                    mergedData[counter++] = position[j];
                }
            }

            counter = 0;
            List<int[]> alldata = new List<int[]>();
            for (int i = 0; i < nGroups; ++i)
            {
                int[] data = new int[groupSize];
                for (int j = 0; j < groupSize; ++j)
                {
                    data[j] = mergedData[counter++];
                }
                alldata.Add(data);
            }
            return alldata;
        }

        static public List<int[]> GetRandomGroups(int N, int size, int layers)
        {
            int nGroups = (int)Math.Pow(2.0, layers - 1.0);

            //. get all combinations
            List<int[]> combinations = new List<int[]>();
            int ii = 0;
            byte[] state = new byte[size];
            for (int jj = 0; jj < size; ++jj) state[jj] = 0;
            while (true)
            {
                int[] row = new int[size];
                if (0 == ii) row[0] = 0;
                for (int jj = 0; jj < size; ++jj)
                {
                    if (0 != ii || 0 != jj)
                    {
                        if (0 == state[jj]) row[jj] = row[jj - 1] + 1;
                        if (1 == state[jj]) row[jj] = combinations[ii - 1][jj] + 1;
                        if (2 == state[jj]) row[jj] = combinations[ii - 1][jj];
                        if ((N - size + jj) == row[jj])
                        {
                            if (jj > 0) if (2 == state[jj - 1]) state[jj - 1] = 1;
                            state[jj] = 0;
                        }
                    }
                }
                combinations.Add(row);
                if (row[size - 1] < N - 1)
                {
                    for (int jj = 0; jj < size - 1; ++jj) state[jj] = 2;
                    state[size - 1] = 1;
                }
                byte maxState = 0;
                for (int jj = 0; jj < size; ++jj)
                {
                    if (state[jj] > maxState) maxState = state[jj];
                }
                ++ii;
                if (0 == maxState) break;
            }
            int nCombinations = ii;

            //. get random indices, without repetition within a block of size N
            Random rnd = new Random();
            List<int> positions = GetOrderedArray(nCombinations);
            int[] groupInds = new int[nGroups];
            for (int i = 0; i < nGroups; ++i)
            {
                int random = rnd.Next() % positions.Count;
                groupInds[i] = positions[random];
                positions.RemoveAt(random);
                if (0 == positions.Count)
                {
                    positions = GetOrderedArray(nCombinations);
                }
            }

            //. fill groups out of combinations
            List<int[]> groups = new List<int[]>();
            for (int i = 0; i < nGroups; ++i)
            {
                groups.Add(combinations[groupInds[i]]);
            }

            return groups;
        }

        static public double GetMin(double[] x)
        {
            double min = x[0];
            for (int i = 0; i < x.Length; ++i)
            {
                if (min > x[i]) min = x[i];
            }
            return min;
        }

        static public double GetStDev(double[] x, double? mean)
        {
            if (null == mean) mean = GetMean(x);
            double stdev = 0.0;
            foreach (double d in x)
            {
                stdev += (d - (double)(mean)) * (d - (double)(mean));
            }
            stdev /= (double)(x.Length);
            stdev = Math.Sqrt(stdev);
            return stdev;
        }

        static public double GetMean(double[] x)
        {
            double mean = 0.0;
            foreach (double d in x)
            {
                mean += d;
            }
            return mean / (double)(x.Length);
        }

        static public double GetMax(double[] x)
        {
            double max = x[0];
            for (int i = 0; i < x.Length; ++i)
            {
                if (max < x[i]) max = x[i];
            }
            return max;
        }

        static private void Swap(ref int n1, ref int n2)
        {
            int v1 = n1;
            n1 = n2;
            n2 = v1;
        }

        public static int[] GetOrdered(int size)
        {
            int[] position = new int[size];
            for (int i = 0; i < size; ++i)
            {
                position[i] = i;
            }
            return position;
        }

        public static int[] Shuffle(int size)
        {
            int[] position = new int[size];
            for (int i = 0; i < size; ++i)
            {
                position[i] = i;
            }
            for (int i = 0; i < position.Length; ++i)
            {
                int plusPos = _rnd.Next() % position.Length;
                int next = i + plusPos;
                if (next > position.Length - 1) next -= (position.Length - 1);

                int el1 = position[i];
                int el2 = position[next];
                Swap(ref el1, ref el2);
                position[i] = el1;
                position[next] = el2;
            }
            return position;
        }

        static public double PearsonCorrelation(double[] x, double[] y)
        {
            int length = x.Length;
            if (length > y.Length)
            {
                length = y.Length;
            }

            double xy = 0.0;
            double x2 = 0.0;
            double y2 = 0.0;
            for (int i = 0; i < length; ++i)
            {
                xy += x[i] * y[i];
                x2 += x[i] * x[i];
                y2 += y[i] * y[i];
            }
            xy /= (double)(length);
            x2 /= (double)(length);
            y2 /= (double)(length);
            double xav = 0.0;
            for (int i = 0; i < length; ++i)
            {
                xav += x[i];
            }
            xav /= length;
            double yav = 0.0;
            for (int i = 0; i < length; ++i)
            {
                yav += y[i];
            }
            yav /= length;
            double ro = xy - xav * yav;
            ro /= Math.Sqrt(x2 - xav * xav);
            ro /= Math.Sqrt(y2 - yav * yav);
            return ro;
        }

        static List<int> GetOrderedArray(int n)
        {
            List<int> array = new List<int>();
            for (int i = 0; i < n; ++i)
            {
                array.Add(i);
            }
            return array;
        }

        static public void ShowGroups(List<int[]> groups)
        {
            Console.WriteLine("The groups of input parameters for layer 0:");
            foreach (int[] data in groups)
            {
                foreach (int d in data)
                {
                    Console.Write(" {0}", d);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static private bool IsValid(int[] group)
        {
            int max = group[0];
            for (int i = 1; i < group.Length; ++i)
            {
                if (max < group[i]) max = group[i];
            }
            int[] stat = new int[max + 1];
            for (int i = 0; i < stat.Length; ++i)
            {
                stat[i] = 0;
            }
            for (int i = 0; i < group.Length; ++i)
            {
                ++stat[group[i]];
                if (stat[group[i]] > 1) return false;
            }
            return true;
        }

        public static void TestPLL()
        {
            double xmin = -1.0;
            double xmax = 1.0;
            PLL pll = new PLL(16, 0.0, 0.1);

            Random rnd = new Random();

            for (int step = 0; step < 10; ++step)
            {
                double diff = 0.0;
                int cnt = 0;
                for (int i = 0; i < 10000; ++i)
                {
                    double x = rnd.Next() % 100 / 100.0;
                    x *= (xmax - xmin);
                    x += xmin;
                    double y = x * x;
                    double model = pll.GetFunction(x);

                    diff += (y - model) * (y - model);
                    ++cnt;

                    pll.Update(x, (y - model), 0.05, false);
                }
 
                diff /= cnt;
                diff = Math.Sqrt(diff);
                diff /= (xmax - xmin);
                Console.WriteLine("Relative difference {0:0.0000}, step {1}", diff, step);

                if (3 == step)
                {
                    pll.Resize(25);
                }
            }
        }

        public static U InitializeU(int nt, int[] nx, double[] xmin, double[] xmax, double ymin, double ymax, InitType it, int[] group)
        {
            U u = new U(group);
            ymin /= (double)(nt);
            ymax /= (double)(nt);
            u.Initialize(nt, nx, xmin, xmax);
            if (InitType.RANDOM == it)
            {
                u.SetRandom(ymin, ymax);
            }
            if (InitType.CONST == it)
            {
                u.SetConstant(-1.0);
            }
            if (InitType.LINEAR == it)
            {
                u.SetLinear(ymin, ymax);
            }
            return u;
        }

        public static U InitializeU(int nt, int nx, double xmin, double xmax, double ymin, double ymax, InitType it, int[] group)
        {
            U u = new U(group);
            double[] mins = new double[nt];
            double[] maxs = new double[nt];
            for (int i = 0; i < nt; ++i)
            {
                mins[i] = xmin;
                maxs[i] = xmax;
            }

            ymin /= (double)(nt);
            ymax /= (double)(nt);
            u.Initialize(nt, nx, mins, maxs);
            if (InitType.RANDOM == it)
            {
                u.SetRandom(ymin, ymax);
            }
            if (InitType.CONST == it)
            {
                u.SetConstant(-1.0);
            }
            if (InitType.LINEAR == it)
            {
                u.SetLinear(ymin, ymax);
            }
            return u;
        }

        public static int[] Histogram(double[] y, int blocks)
        {
            double min = GetMin(y);
            double max = GetMax(y);

            int[] histo = new int[blocks];
            for (int i = 0; i < histo.Length; ++i) histo[i] = 0;
            double delta = (max - min);
            delta /= (double)(blocks);

            for (int i = 0; i < y.Length; ++i)
            {
                for (int j = 0; j < blocks; ++j)
                {
                    if (y[i] >= min + j * delta && y[i] <= min + (j + 1) * delta)
                    {
                        ++histo[j];
                        break;
                    }
                }
            }

            Console.WriteLine("--- Histogram ---");
            for (int i = 0; i < blocks; ++i)
            {
                Console.Write("{0} ", histo[i]);
            }
            Console.WriteLine("min/max {0:0.0000} {1:0.0000}", min, max);

            return histo;
        }

        /////// Kolmogorov-Smirnov test
        private static double GetFunctionValue(double[] f, double v)
        {
            if (v > f[f.Length - 1]) return 1.0;
            int n = 0;
            for (int i = 0; i < f.Length; ++i)
            {
                if (f[i] > v)
                {
                    n = i;
                    break;
                }
            }
            double delta = 1.0 / (double)(f.Length);
            return (double)(n) * delta;
        }

        public static bool KSTRejected005(double[] x, double[] y)
        {
            double D = Double.MinValue;
            double deltaX = 1.0 / (double)(x.Length);
            for (int i = 0; i < x.Length; ++i)
            {

                double f1 = (double)(i + 1) * deltaX;
                double f2 = GetFunctionValue(y, x[i]);
                double diff = Math.Abs(f1 - f2);
                if (diff > D)
                {
                    D = diff;
                }
            }
            double deltaY = 1.0 / (double)(y.Length);
            for (int i = 0; i < y.Length; ++i)
            {
                double f1 = GetFunctionValue(x, y[i]);
                double f2 = (double)(i + 1) * deltaY;
                double diff = Math.Abs(f1 - f2);
                if (diff > D)
                {
                    D = diff;
                }
            }
            double D_critical = 1.358 * Math.Sqrt(1.0 / (double)(x.Length) + 1.0 / (double)(y.Length));
            if (D >= D_critical) return true;
            else return false;
        }
    }
}
