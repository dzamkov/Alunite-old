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
        /// Creates an identical curve with a higher order.
        /// </summary>
        public Curve<T> Elevate(int Order)
        {
            int target = Order + 1;
            T[] cpoints = this._Points;
            while (cpoints.Length < target)
            {
                int cur = cpoints.Length;
                T[] npoints = new T[cur + 1];
                npoints[0] = cpoints[0];
                npoints[cur] = cpoints[cur - 1];

                double dcur = (double)cur;
                for (int i = 1; i < cur; i++)
                {
                    npoints[i] = cpoints[i - 1].Multiply((double)i /  dcur).Add(cpoints[i].Multiply((dcur - (double)i) / dcur));
                }
                cpoints = npoints;
            }
            return new Curve<T>(cpoints);
        }

        /// <summary>
        /// Gets the order (complexity, degree) of this bezier curve. 0 is the minimum order for
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
        /// Gets the control points for this curve. Note that the array should not be modified.
        /// </summary>
        public T[] Points
        {
            get
            {
                return this._Points;
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
        /// Gets the first value in this curve.
        /// </summary>
        public T Initial
        {
            get
            {
                return this._Points[0];
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
        /// Creates a curve with a constant value.
        /// </summary>
        public static Curve<T> Constant<T>(T Value)
             where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
        {
            return new Curve<T>(new T[] { Value });
        }

        /// <summary>
        /// Creates a curve that varies linearly between two values.
        /// </summary>
        public static Curve<T> Linear<T>(T Start, T End)
            where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
        {
            return new Curve<T>(new T[] { Start, End });
        }

        /// <summary>
        /// Creates a curve with a constant value.
        /// </summary>
        public static Curve<Scalar> Constant(double Value)
        {
            return Constant<Scalar>(Value);
        }

        /// <summary>
        /// Creates a curve that varies linearly between two values.
        /// </summary>
        public static Curve<Scalar> Linear(double Start, double End)
        {
            return Linear<Scalar>(Start, End);
        }

        /// <summary>
        /// Gets the intergral of the given curve starting at the specified initial value.
        /// </summary>
        public static Curve<T> Integral<T>(Curve<T> Curve, T Initial)
            where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
        {
            T[] cpoints = Curve.Points;
            T[] ipoints = new T[cpoints.Length + 1];
            double scale = 1.0 / (double)cpoints.Length;

            ipoints[0] = Initial;
            for (int t = 0; t < cpoints.Length; t++)
            {
                Initial = Initial.Add(cpoints[t].Multiply(scale));
                ipoints[t + 1] = Initial;
            }
            return new Curve<T>(ipoints);
        }

        /// <summary>
        /// Gets the derivative of the given curve.
        /// </summary>
        public static Curve<T> Derivative<T>(Curve<T> Curve)
            where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
        {
            T[] cpoints = Curve.Points;
            int size = cpoints.Length;
            if (size > 1)
            {
                T[] ipoints = new T[size - 1];
                double scale = (double)(size - 1);
                for (int t = 0; t < ipoints.Length; t++)
                {
                    ipoints[t] = cpoints[t + 1].Subtract(cpoints[t]).Multiply(scale);
                }
                return new Curve<T>(ipoints);
            }
            else
            {
                return Constant(default(T));
            }
        }

        /// <summary>
        /// Sets both curves to have the same order without changing their content.
        /// </summary>
        public static void Align<T>(ref Curve<T> A, ref Curve<T> B)
            where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
        {
            if (A.Order < B.Order)
            {
                A = A.Elevate(B.Order);
            }
            else
            {
                B = B.Elevate(A.Order);
            }
        }

        /// <summary>
        /// Gets the sum of two curves.
        /// </summary>
        public static Curve<T> Add<T>(Curve<T> A, Curve<T> B)
            where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
        {
            Align(ref A, ref B);
            T[] apoints = A.Points;
            T[] bpoints = B.Points;
            T[] npoints = new T[apoints.Length];
            for (int t = 0; t < npoints.Length; t++)
            {
                npoints[t] = apoints[t].Add(bpoints[t]);
            }
            return new Curve<T>(npoints);
        }

        /// <summary>
        /// Gets the difference of two curves.
        /// </summary>
        public static Curve<T> Subtract<T>(Curve<T> A, Curve<T> B)
            where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
        {
            Align(ref A, ref B);
            T[] apoints = A.Points;
            T[] bpoints = B.Points;
            T[] npoints = new T[apoints.Length];
            for (int t = 0; t < npoints.Length; t++)
            {
                npoints[t] = apoints[t].Subtract(bpoints[t]);
            }
            return new Curve<T>(npoints);
        }

        /// <summary>
        /// Scales a curve by the specified multiplier.
        /// </summary>
        public static Curve<T> Scale<T>(Curve<T> Curve, double Multiplier)
            where T : IAdditive<T, T>, IMultiplicative<T, Scalar>
        {
            T[] cpoints = Curve.Points;
            T[] npoints = new T[cpoints.Length];
            for (int t = 0; t < npoints.Length; t++)
            {
                npoints[t] = cpoints[t].Multiply(Multiplier);
            }
            return new Curve<T>(npoints);
        }

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