using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UQ
{
    interface IDataGenerator
    {
        public double Formula(double[] x);
        public double[] GetRandomInput();
        public double[] AddNoise(double[] x, double magnitude, out double datanorm, out double errornorm);
    }

    class DataGenerator1: IDataGenerator
    {
        Random _rnd = new Random();

        public double Formula(double[] x)
        {
            //y = (1/pi)*(2+2*x3)*(1/3)*(atan(20*exp(x5)*(x1-0.5+x2/6))+pi/2) + (1/pi)*(2+2*x4)*(1/3)*(atan(20*exp(x5)*(x1-0.5-x2/6))+pi/2);
            double pi = 3.14159265359;
            if (5 != x.Length)
            {
                Console.WriteLine("Formala error");
                Environment.Exit(0);
            }
            double y = (1.0 / pi);
            y *= (2.0 + 2.0 * x[2]);
            y *= (1.0 / 3.0);
            y *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 + x[1] / 6.0)) + pi/2.0;

            double z = (1.0 / pi);
            z *= (2.0 + 2.0 * x[3]);
            z *= (1.0 / 3.0);
            z *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 - x[1] / 6.0)) + pi/2.0;
 
            return y + z;
        }

        public double[] GetRandomInput()
        {
            double[] x = new double[5];
            x[0] = (_rnd.Next() % 100) / 100.0;
            x[1] = (_rnd.Next() % 100) / 100.0;
            x[2] = (_rnd.Next() % 100) / 100.0;
            x[3] = (_rnd.Next() % 100) / 100.0;
            x[4] = (_rnd.Next() % 100) / 100.0;
            return x;
        }

        public double[] AddNoise(double[] x, double magnitude, out double datanorm, out double errornorm)
        {
            double[] z = new double[5];
            z[0] = x[0] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[1] = x[1] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[2] = x[2] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[3] = x[3] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[4] = x[4] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);

            datanorm = 0.0;
            foreach (double d in x)
            {
                datanorm += d * d;
            }
            datanorm /= x.Length;
            datanorm = Math.Sqrt(datanorm);

            errornorm = 0.0;
            for (int i = 0; i < x.Length; ++i)
            {
                errornorm += (x[i] - z[i]) * (x[i] - z[i]);
            }
            errornorm /= x.Length;
            errornorm = Math.Sqrt(errornorm);

            return z;
        }
    }

    class DataGenerator2: IDataGenerator
    {
        Random _rnd = new Random();

        public double Formula(double[] x)
        {
            if (5 != x.Length)
            {
                Console.WriteLine("Formala error");
                Environment.Exit(0);
            }
            double y = Math.Abs(Math.Pow(Math.Sin(x[1]), x[0]) - 1.0 / Math.Exp(x[2])) / x[3] + x[4] * Math.Cos(x[4]);
            return y;
        }

        public double[] GetRandomInput()
        {
            double[] x = new double[5];
            x[0] = (_rnd.Next() % 100) / 100.0;
            x[1] = (_rnd.Next() % 100) / 100.0 * 3.14 / 2.0;
            x[2] = (_rnd.Next() % 50) / 100.0 + 1;
            x[3] = (_rnd.Next() % 100) / 100.0 + 0.4;
            x[4] = (_rnd.Next() % 100) / 200.0;
            return x;
        }

        public double[] AddNoise(double[] x, double magnitude, out double datanorm, out double errornorm)
        {
            double[] xx = new double[x.Length];
            for (int j = 0; j < x.Length; ++j)
            {
                xx[j] = x[j] + _rnd.Next() % 100 / 100.0 * magnitude - magnitude / 2.0;
            }
            if (xx[0] < 0.0) xx[0] = 0.0;
            if (xx[1] < 0.0) xx[1] = 0.0;
            if (xx[2] < 1.0) xx[2] = 1.0;
            if (xx[3] < 0.4) xx[3] = 0.4;
            if (xx[4] < 0.0) xx[4] = 0.0;

            if (xx[0] > 1.0) xx[0] = 1.0;
            if (xx[1] > 3.14 / 2.0) xx[1] = 3.14 / 2.0;
            if (xx[2] > 1.5) xx[2] = 1.5;
            if (xx[3] > 1.4) xx[3] = 1.4;
            if (xx[4] > 0.5) xx[4] = 0.5;

            datanorm = 0.0;
            foreach (double d in x)
            {
                datanorm += d * d;
            }
            datanorm /= x.Length;
            datanorm = Math.Sqrt(datanorm);

            errornorm = 0.0;
            for (int i = 0; i < x.Length; ++i)
            {
                errornorm += (x[i] - xx[i]) * (x[i] - xx[i]);
            }
            errornorm /= x.Length;
            errornorm = Math.Sqrt(errornorm);

            return xx;
        }
    }

    class DataHolder
	{
        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        public List<int> _id = new List<int>();
        public double[] _xmin = null;
        public double[] _xmax = null;
        public double _targetMin;
        public double _targetMax;
        public double _noise = 0.0;
        private IDataGenerator iData = new DataGenerator2(); //this is switch between different datasets

        public double[] GetRandomInput()
        {
            return iData.GetRandomInput();
        }

        public double[] GetStatData(double[] x, int N)
        {
            double[] y = new double[N];

            for (int i = 0; i < N; ++i)
            {
                double datanorm;
                double errornorm;
                y[i] = iData.Formula(iData.AddNoise(x, _noise, out datanorm, out errornorm));
            }

            return y;
        }

        public void BuildFormulaData(double noise, int N)
        {
            _noise = noise;
            _inputs.Clear();
            _target.Clear();

            double alldatanorm = 0.0;
            double allerrornorm = 0.0;

            for (int i = 0; i < N; ++i)
            {
                double datanorm;
                double errornorm;
                double[] x = GetRandomInput();
                double y = iData.Formula(iData.AddNoise(x, noise, out datanorm, out errornorm));

                alldatanorm += datanorm;
                allerrornorm += errornorm;
              
                _inputs.Add(x);
                _target.Add(y);
                _id.Add(0);
            }
            FindMinMax();

            Console.WriteLine("Data is generated, average relative error {0:0.0000}", allerrornorm / alldatanorm);
        }

        private void FindMinMax()
        {
            int size = _inputs[0].Length;
            _xmin = new double[size];
            _xmax = new double[size];

            for (int i = 0; i < size; ++i)
            {
                _xmin[i] = double.MaxValue;
                _xmax[i] = double.MinValue;
            }

            for (int i = 0; i < _inputs.Count; ++i)
            {
                for (int j = 0; j < _inputs[i].Length; ++j)
                {
                    if (_inputs[i][j] < _xmin[j]) _xmin[j] = _inputs[i][j];
                    if (_inputs[i][j] > _xmax[j]) _xmax[j] = _inputs[i][j];
                }

            }

            _targetMin = double.MaxValue;
            _targetMax = double.MinValue;
            for (int j = 0; j < _target.Count; ++j)
            {
                if (_target[j] < _targetMin) _targetMin = _target[j];
                if (_target[j] > _targetMax) _targetMax = _target[j];
            }
        }

        public double[] GetInputMin()
        {
            return _xmin;
        }

        public double[] GetInputMax()
        {
            return _xmax;
        }

        public double GetTargetMax()
        {
            return _targetMax;
        }

        public double GetTargetMin()
        {
            return _targetMin;
        }

        public int GetNumberOfInputs()
        {
            double[] sample = _inputs[0];
            return sample.Length;
        }
    }
}

