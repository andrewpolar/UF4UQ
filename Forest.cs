using System;
using System.Collections.Generic;
using System.Text;

namespace UQ
{
    class Forest
    {
        public Forest(DataHolder dh)
        {
            _dh = dh;
        }

        private DataHolder _dh = null;
        Unod[] _forest = null;
        int _NN = -1;

        private void SplitData(Unod tree, int label, int leftLabel, int rightLabel)
        {
            List<double> diff = new List<double>();
            for (int i = 0; i < _dh._target.Count; ++i)
            {
                if (label == _dh._id[i])
                {
                    double y = _dh._target[i];
                    double[] x = _dh._inputs[i];
                    double model = tree.GetResult(x);
                    diff.Add(y - model);
                }
            }

            double[] array = diff.ToArray();
            Array.Sort(array);
            int median = array.Length / 2;
            double medianDiff = array[median];

            for (int i = 0; i < _dh._target.Count; ++i)
            {
                if (label == _dh._id[i])
                {
                    double y = _dh._target[i];
                    double[] x = _dh._inputs[i];
                    double model = tree.GetResult(x);
                    if (y - model <= medianDiff) _dh._id[i] = leftLabel;
                    else _dh._id[i] = rightLabel;
                }
            }
            diff.Clear();
        }

        private Unod IdentifyTree(int label, Unod initialTree = null)
        {
            double mu = 0.05; //regularization
            TreeModel tm = new TreeModel(4, 3, 10, _dh, label);
            if (null == initialTree)
            {
                initialTree = tm.Initialize();
            }
            Unod tree = tm.Run(initialTree, 100, mu);
            return tree;
        }

        public void BuildForest(int N)
        {
            if (!(2 == N || 4 == N || 8 == N || 16 == N))
            {
                Console.WriteLine("Fatal. The forest can be built with 2, 4, 8, or 16 trees");
                Environment.Exit(0);
            }

            _NN = N;

            //Identify forest
            _forest = new Unod[31];
            _forest[0] = IdentifyTree(0);

            SplitData(_forest[0], 0, 1, 2);
            _forest[1] = IdentifyTree(1, _forest[0]);
            _forest[2] = IdentifyTree(2, _forest[0]);

            if (2 == _NN) return;

            SplitData(_forest[1], 1, 3, 4);
            _forest[3] = IdentifyTree(3, _forest[0]);
            _forest[4] = IdentifyTree(4, _forest[0]);

            SplitData(_forest[2], 2, 5, 6);
            _forest[5] = IdentifyTree(5, _forest[0]);
            _forest[6] = IdentifyTree(6, _forest[0]);

            if (4 == _NN) return;

            SplitData(_forest[3], 3, 7, 8);
            _forest[7] = IdentifyTree(7, _forest[0]);
            _forest[8] = IdentifyTree(8, _forest[0]);

            SplitData(_forest[4], 4, 9, 10);
            _forest[9] = IdentifyTree(9, _forest[0]);
            _forest[10] = IdentifyTree(10, _forest[0]);

            SplitData(_forest[5], 5, 11, 12);
            _forest[11] = IdentifyTree(11, _forest[0]);
            _forest[12] = IdentifyTree(12, _forest[0]);

            SplitData(_forest[6], 6, 13, 14);
            _forest[13] = IdentifyTree(13, _forest[0]);
            _forest[14] = IdentifyTree(14, _forest[0]);

            if (8 == _NN) return;

            SplitData(_forest[7], 7, 15, 16);
            _forest[15] = IdentifyTree(15, _forest[0]);
            _forest[16] = IdentifyTree(16, _forest[0]);

            SplitData(_forest[8], 8, 17, 18);
            _forest[17] = IdentifyTree(17, _forest[0]);
            _forest[18] = IdentifyTree(18, _forest[0]);

            SplitData(_forest[9], 9, 19, 20);
            _forest[19] = IdentifyTree(19, _forest[0]);
            _forest[20] = IdentifyTree(20, _forest[0]);

            SplitData(_forest[10], 10, 21, 22);
            _forest[21] = IdentifyTree(21, _forest[0]);
            _forest[22] = IdentifyTree(22, _forest[0]);

            SplitData(_forest[11], 11, 23, 24);
            _forest[23] = IdentifyTree(23, _forest[0]);
            _forest[24] = IdentifyTree(24, _forest[0]);

            SplitData(_forest[12], 12, 25, 26);
            _forest[25] = IdentifyTree(25, _forest[0]);
            _forest[26] = IdentifyTree(26, _forest[0]);

            SplitData(_forest[13], 13, 27, 28);
            _forest[27] = IdentifyTree(27, _forest[0]);
            _forest[28] = IdentifyTree(28, _forest[0]);

            SplitData(_forest[14], 14, 29, 30);
            _forest[29] = IdentifyTree(29, _forest[0]);
            _forest[30] = IdentifyTree(30, _forest[0]);
        }

        public double[] GetSortedVotes(double[] x)
        {
            if (_NN < 0)
            {
                Console.WriteLine("Fatal. The tree is not identified");
                Environment.Exit(0);
            }

            double[] v = null;
            if (2 == _NN)
            {
                v = new double[2];
                for (int i = 0; i < v.Length; ++i)
                {
                    v[i] = _forest[i + 1].GetResult(x);
                }
            }
            else if (4 == _NN)
            {
                v = new double[4];
                for (int i = 0; i < v.Length; ++i)
                {
                    v[i] = _forest[i + 3].GetResult(x);
                }
            }
            else if (8 == _NN)
            {
                v = new double[8];
                for (int i = 0; i < v.Length; ++i)
                {
                    v[i] = _forest[i + 7].GetResult(x);
                }
            }
            else
            {
                v = new double[16];
                for (int i = 0; i < v.Length; ++i)
                {
                    v[i] = _forest[i + 15].GetResult(x);
                }
            }

            Array.Sort(v);
            return v;
        }

        public double GetRootScalar(double[] x)
        {
            return _forest[0].GetResult(x);
        }
    }
}
