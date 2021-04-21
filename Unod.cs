using System;
using System.Collections.Generic;
using System.Text;

namespace UQ
{
    class UHolder
    {
        public static List<Unod> Entry = new List<Unod>();
    }

    [Serializable]
    public class Unod
    {
        private Unod _Left = null;
        private Unod _Right = null;
        private int _Level = -1;
        private int _Limit = -1;
        private double _delta;
        private double[] _x = new double[2];
        public U _u = null;
  
        public Unod(int Level, int Limit)
        {
            _Level = Level + 1;
            _Limit = Limit;
            if (_Limit > _Level)
            {
                _Left = new Unod(_Level, Limit);
                _Right = new Unod(_Level, Limit);
            }
            else
            {
                UHolder.Entry.Add(this);
            }
        }

        public void InitializeU(int nt, int nx, double xmin, double xmax, double ymin, double ymax)
        {
            if (_Limit <= _Level) return;
            _u = Static.InitializeU(nt, nx, ymin, ymax, ymin, ymax, InitType.LINEAR, new int[] { 0, 1 });
            _Left.InitializeU(nt, nx, ymin, ymax, ymin, ymax);
            _Right.InitializeU(nt, nx, ymin, ymax, ymin, ymax);
        }

        public double GetResult(double[] inputs)
        {
            if (null == _Left && null == _Right)
            {
                double rtn = _u.GetU(inputs);
                return rtn;
            }
            else
            {
                _x[0] = _Left.GetResult(inputs);
                _x[1] = _Right.GetResult(inputs);
                double rtn = _u.GetU(_x);
                return rtn;
            }
        }

        public void FindDeltas(double delta)
        {
            _delta = delta;
            if (null != _Left && null != _Right)
            {
                double derrivative0 = _u.GetDerrivative(0, _x[0]);
                double derrivative1 = _u.GetDerrivative(1, _x[1]);

                double delta0 = _delta;
                double delta1 = _delta;
                if (derrivative0 < 0.0) delta0 = -delta0;
                if (derrivative1 < 0.0) delta1 = -delta1;

                _Left.FindDeltas(delta0);
                _Right.FindDeltas(delta1);
            }
        }

        public void Update(double[] inputs, double mu)
        {
            if (null == _Left && null == _Right)
            {
                _u.Update(_delta, inputs, mu, true);
            }
            else
            {
                _u.Update(_delta, _x, mu, false);
                _Left.Update(inputs, mu);
                _Right.Update(inputs, mu);
            }
        }

        public int GetLevel()
        {
            return _Level;
        }
    }
}
