using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace UQ
{
    public class TreeHolder
    {
        public IFormatter formatter = new BinaryFormatter();
        MemoryStream _stream = null;

        public void SerializeTree(Unod Root)
        {
            if (null != _stream)
            {
                _stream.Dispose();
                _stream = null;
            }
            _stream = new MemoryStream();
            formatter.Serialize(_stream, Root);
        }

        public Unod DeserializeTree()
        {
            if (null == _stream) return null;
            _stream.Seek(0, SeekOrigin.Begin);
            Unod obj = (Unod)formatter.Deserialize(_stream);
            return obj;
        }
    }

    class TreeModel
    {
        private int _TreeLayers;
        private int _InputSampleSize;
        private int _nx;
        private DataHolder _dh;
        private int _id;
 
        public TreeModel(int TreeLayers, int InputSampleSize, int nx, DataHolder dh, int id)
        {
            _TreeLayers = TreeLayers;
            _InputSampleSize = InputSampleSize;
            _nx = nx;
            _dh = dh;
            _id = id;
        }

        private double[] GetMinMax(double[] minmax, int[] group)
        {
            double[] rtn = new double[group.Length];
            int i = 0;
            foreach (int n in group)
            {
                rtn[i] = minmax[n];
                ++i;
            }
            return rtn;
        }

        private int[] GetBlocks(int[] NX, int[] group)
        {
            int[] rtn = new int[group.Length];
            int i = 0;
            foreach (int n in group)
            {
                rtn[i] = NX[n];
                ++i;
            }
            return rtn;
        }

        private void InitializeEntries(DataHolder dh, int InputSampleSize, int nTreeLayers, int[] NX)
        {
            int nInputs = dh.GetNumberOfInputs();
            double targetMax = dh.GetTargetMax();
            double targetMin = dh.GetTargetMin();
            double[] xmin = dh.GetInputMin();
            double[] xmax = dh.GetInputMax();     

            List<int[]> groups = Static.GetRandomGroups(nInputs, InputSampleSize, nTreeLayers);
            int index = 0;
            foreach (Unod unod in UHolder.Entry)
            {
                double[] min = GetMinMax(xmin, groups[index]);
                double[] max = GetMinMax(xmax, groups[index]);
                int[] nx = GetBlocks(NX, groups[index]);
                unod._u = Static.InitializeU(InputSampleSize, nx, min, max, targetMin, targetMax, InitType.RANDOM, groups[index]);
                ++index;
            }
            groups.Clear();
            Console.WriteLine("Number of leafs {0}", index);
        }

        private double EstimateTreeAccuracy(Unod Root, bool bShow = false)
        {
            double error = 0.0;
            double min = double.MaxValue;
            double max = double.MinValue;
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            int cnt = 0;
            for (int i = 0; i < _dh._target.Count; ++i)
            {
                if (_dh._id[i] != _id) continue;
                double[] inputs = _dh._inputs[i];
                double z = _dh._target[i];
                double model = Root.GetResult(inputs);
                x.Add(model);
                y.Add(z);
                error += (z - model) * (z - model);
                if (z < min) min = z;
                if (z > max) max = z;
                ++cnt;
            }
            double pearson = Static.PearsonCorrelation(x.ToArray(), y.ToArray());
            error /= (double)(cnt);
            error = Math.Sqrt(error);
            error /= (max - min);
            if (bShow) Console.WriteLine("Relative error for tree {0:0.0000}, pearson {1:0.0000}", error, pearson);
            return error;
        }

        private void Identification(Unod Root, int Steps, TreeHolder th, double mu)
        {
            double minError = double.MaxValue;
            for (int step = 0; step < Steps; ++step)
            {
                for (int i = 0; i < _dh._target.Count; ++i)
                {
                    if (_dh._id[i] != _id) continue;
                    double[] inputs = _dh._inputs[i];
                    double z = _dh._target[i];
                    double model = Root.GetResult(inputs);
                    double delta = z - model;
                    Root.FindDeltas(delta);
                    Root.Update(inputs, mu);
                }
                double error = EstimateTreeAccuracy(Root);
                if (error < minError)
                {
                    th.SerializeTree(Root);
                    minError = error;
                }
            }
        }

        public Unod Initialize()
        {
            UHolder.Entry.Clear();
            Unod Root = new Unod(0, _TreeLayers);

            int number_of_inputs = _dh._xmin.Length;
            int[] number_of_linear_blocks = new int[number_of_inputs];
            for (int i = 0; i < number_of_inputs; ++i)
            {
                number_of_linear_blocks[i] = 10;
            }
            InitializeEntries(_dh, _InputSampleSize, _TreeLayers, number_of_linear_blocks);
            Root.InitializeU(2, _nx, 0.0, 0.0, _dh.GetTargetMin(), _dh.GetTargetMax());
            return Root;
        }

        public Unod Run(Unod Root, int steps, double mu)
        {
            TreeHolder treeHolder = new TreeHolder();
            treeHolder.SerializeTree(Root);
            Unod treeCopy = treeHolder.DeserializeTree();

            //Identification
            DateTime start = DateTime.Now;
            TreeHolder th = new TreeHolder();
            Identification(treeCopy, steps, th, mu);
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            double time = duration.Minutes * 60.0 + duration.Seconds + duration.Milliseconds / 1000.0;

            //End result, the model is tested on validation data
            Console.WriteLine("---------------------------------------------- time {0:0.0000}", time);
            Console.WriteLine("Best tree");
            Unod BestTree = th.DeserializeTree();
            EstimateTreeAccuracy(BestTree, true);
            Console.WriteLine();

            return BestTree;
        }
    }
}
