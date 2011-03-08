using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A continous function with a domain of reals bounded between 0.0 and 1.0 defined using a bezier curve.
    /// </summary>
    public sealed class Curve<T>
        where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
    {
        public Curve(T[] Points)
        {
            this._Points = Points;
        }

        /// <summary>
        /// Gets the order (complexity) of this bezier curve. 0 is the minimum order for
        /// a valid curve.
        /// </summary>
        public int Order
        {
            get
            {
                return this._Points.Length - 1;
            }
        }

        /// <summary>
        /// Gets the value of the curve with the specified parameter between 0.0 and 1.0.
        /// </summary>
        public T this[double Param]
        {
            get
            {
                T[] coff = this._GetCoffecients();
                T res = coff[0];
                double apar = Param;
                for (int i = 1; i < coff.Length; i++)
                {
                    res = res.Add(coff[i].Multiply(apar));
                    apar *= Param;
                }
                return res;
            }
        }

        /// <summary>
        /// Gets the coffecients that can be multiplied by corresponding degrees of the parameter to get the result at the parameter.
        /// </summary>
        private T[] _GetCoffecients()
        {
            if (this._Coff == null)
            {
                int size = this._Points.Length;
                this._Coff = new T[size];
                double[] bcoff = Curve.GetBezierCoffecients(this.Order);
                for (int t = 0; t < size; t++)
                {
                    int s = t * size;
                    T coff = this._Points[0].Multiply(bcoff[s]);
                    for (int i = 1; i < size; i++)
                    {
                        s++;
                        coff = coff.Add(this._Points[i].Multiply(bcoff[s]));
                    }
                    this._Coff[t] = coff;
                }
            }
            return this._Coff;
        }

        private T[] _Points;
        private T[] _Coff;
    }

    /// <summary>
    /// Contains functions related to curves.
    /// </summary>
    public static class Curve
    {

        /// <summary>
        /// Contains the coffecients needed to evaluate bezier curves of certain orders.
        /// </summary>
        private static readonly List<double[]> _BezierCoffecients = new List<double[]>();

        /// <summary>
        /// Gets the bezier coffecients for the specified order. They are given as a matrix correlating the degree of
        /// the parameter given to the curve and the control points.
        /// </summary>
        public static double[] GetBezierCoffecients(int Order)
        {
            if (Order < _BezierCoffecients.Count)
            {
                return _BezierCoffecients[Order];
            }
            else
            {
                // Compute the coffecients based on the previous order
                int psize = Order;
                int size = (Order + 1);
                double[] coff = new double[size * size];
                if (Order == 0)
                {
                    coff[0] = 1;
                }
                else
                {
                    double[] precoff = GetBezierCoffecients(Order - 1);
                    for (int x = 0; x < psize; x++)
                    {
                        for (int y = 0; y < psize; y++)
                        {
                            double cur = precoff[x + (y * psize)];
                            coff[x + (y * size) + size] -= cur;
                            coff[x + (y * size) + size + 1] += cur;
                            coff[x + (y * size)] += cur;
                        }
                    }
                }
                _BezierCoffecients.Add(coff);
                return coff;
            }
        }
    }
}