using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains functions and methods related to cubic signals.
    /// </summary>
    public abstract class CubicSignal
    {
        /// <summary>
        /// A vertex for a cubic signal.
        /// </summary>
        public struct Vertex<T>
        {
            public Vertex(double Time, T Value, T Derivative)
            {
                this.Time = Time;
                this.Value = Value;
                this.Derivative = Derivative;
            }

            /// <summary>
            /// The time this vertex occurs at.
            /// </summary>
            public double Time;

            /// <summary>
            /// The value of the signal at this vertex.
            /// </summary>
            public T Value;

            /// <summary>
            /// The first-order derivative at this vertex.
            /// </summary>
            public T Derivative;
        }
    }

    /// <summary>
    /// A continous finite signal defined by a cubic (natural) spline. These signals are best for approximations because they
    /// can not represent many curves exactly but can represent many curves closely and quickly.
    /// </summary>
    public class CubicSignal<T, TContinuum> : ContinuousSignal<T, TContinuum>
        where TContinuum : IContinuum<T>
    {
        public CubicSignal(List<CubicSignal.Vertex<T>> Vertices, TContinuum Continuum)
        {
            this._Vertices = Vertices;
            this._Continuum = Continuum;
        }

        public override T this[double Time]
        {
            get
            {
                CubicSignal.Vertex<T> f, s; double delta, param;
                this.GetInterval(Time, out f, out s, out delta, out param);

                TContinuum ct = this._Continuum;

                T va = f.Value;
                T vb = ct.Add(f.Value, ct.Multiply(f.Derivative, delta / 3.0));
                T vc = ct.Subtract(s.Value, ct.Multiply(s.Derivative, delta / 3.0));
                T vd = s.Value;

                T ve = ct.Mix(va, vb, param);
                T vf = ct.Mix(vb, vc, param);
                T vg = ct.Mix(vc, vd, param);

                T vh = ct.Mix(ve, vf, param);
                T vi = ct.Mix(vf, vg, param);

                return ct.Mix(vh, vi, param);
            }
        }

        public override T GetDerivative(double Time)
        {
            CubicSignal.Vertex<T> f, s; double delta, param;
            this.GetInterval(Time, out f, out s, out delta, out param);

            TContinuum ct = this._Continuum;

            T va = f.Derivative;
            T vb = ct.Multiply(ct.Subtract(s.Value, f.Value), 1.0 / delta);
            T vc = s.Derivative;

            T vd = ct.Mix(va, vb, param);
            T ve = ct.Mix(vb, vc, param);

            return ct.Mix(vd, ve, param);
        }

        public override TContinuum Continuum
        {
            get
            {
                return this._Continuum;
            }
        }

        /// <summary>
        /// Gets a vertex for the signal at the given tim.
        /// </summary>
        public CubicSignal.Vertex<T> GetVertex(double Time)
        {
            return new CubicSignal.Vertex<T>(Time, this[Time], this.GetDerivative(Time));
        }

        /// <summary>
        /// Gets the second-order derivative of this signal at the specified time.
        /// </summary>
        public T GetSecondDerivative(double Time)
        {
            CubicSignal.Vertex<T> f, s; double delta, param;
            this.GetInterval(Time, out f, out s, out delta, out param);

            TContinuum ct = this._Continuum;

            throw new NotImplementedException();
        }

        public override double Length
        {
            get
            {
                return this._Vertices[this._Vertices.Count - 1].Time;
            }
        }

        /// <summary>
        /// Uniformly resamples this signal to have the given amount of vertices.
        /// </summary>
        public CubicSignal<T, TContinuum> Resample(int Amount)
        {
            CubicSignal.Vertex<T> last = this._Vertices[this._Vertices.Count - 1];


            List<CubicSignal.Vertex<T>> vs = new List<CubicSignal.Vertex<T>>();

            double d = last.Time / Amount;
            Amount -= 2;

            vs.Add(this._Vertices[0]);
            for (int i = 0; i < Amount; i++)
            {
                vs.Add(this.GetVertex(d * (i + 1)));
            }
            vs.Add(last);

            return new CubicSignal<T, TContinuum>(vs, this._Continuum);
        }

        /// <summary>
        /// Creates a cubic signal that varies linearly between two values for the given amount of time.
        /// </summary>
        public static CubicSignal<T, TContinuum> Linear(TContinuum Continuum, T First, T Last, double Length)
        {
            double ilen = 1.0 / Length;
            T d = Continuum.Multiply(Continuum.Subtract(Last, First), ilen);
            List<CubicSignal.Vertex<T>> vs = new List<CubicSignal.Vertex<T>>();
            vs.Add(new CubicSignal.Vertex<T>(0.0, First, d));
            vs.Add(new CubicSignal.Vertex<T>(Length, Last, d));

            return new CubicSignal<T, TContinuum>(vs, Continuum);
        }

        /// <summary>
        /// Approximates the product of two signals.
        /// </summary>
        public static CubicSignal<T, TContinuum> Product<TA, TAContinuum, TB, TBContinuum, TMultiplication>(
            CubicSignal<TA, TAContinuum> A,
            CubicSignal<TB, TBContinuum> B,
            TMultiplication Multiplication,
            TContinuum Continuum)
            where TAContinuum : IContinuum<TA>
            where TBContinuum : IContinuum<TB>
            where TMultiplication : IMultiplication<TA, TB, T>
        {
            return BinaryOperation(A, B, (x, y) => Product(x, y, Multiplication, Continuum), Continuum);
        }

        /// <summary>
        /// Approximates the sum of two signals.
        /// </summary>
        public static CubicSignal<T, TContinuum> Sum(CubicSignal<T, TContinuum> A, CubicSignal<T, TContinuum> B, TContinuum Continuum)
        {
            return BinaryOperation(A, B, (x, y) => new CubicSignal.Vertex<T>(x.Time, Continuum.Add(x.Value, y.Value), Continuum.Add(x.Derivative, y.Derivative)), Continuum);
        }

        /// <summary>
        /// Approximates the difference of two signals.
        /// </summary>
        public static CubicSignal<T, TContinuum> Difference(CubicSignal<T, TContinuum> A, CubicSignal<T, TContinuum> B, TContinuum Continuum)
        {
            return BinaryOperation(A, B, (x, y) => new CubicSignal.Vertex<T>(x.Time, Continuum.Subtract(x.Value, y.Value), Continuum.Subtract(x.Derivative, y.Derivative)), Continuum);
        }

        /// <summary>
        /// Performs a binary operation on two signals. The resulting signal will have the length of the smaller of the two given signals.
        /// </summary>
        public static CubicSignal<T, TContinuum> BinaryOperation<TA, TAContinuum, TB, TBContinuum>(
            CubicSignal<TA, TAContinuum> A,
            CubicSignal<TB, TBContinuum> B,
            Func<CubicSignal.Vertex<TA>, CubicSignal.Vertex<TB>, CubicSignal.Vertex<T>> Function,
            TContinuum Continuum)
            where TAContinuum : IContinuum<TA>
            where TBContinuum : IContinuum<TB>
        {
            List<CubicSignal.Vertex<TA>> avs = A._Vertices;
            List<CubicSignal.Vertex<TB>> bvs = B._Vertices;
            List<CubicSignal.Vertex<T>> vs = new List<CubicSignal.Vertex<T>>(avs.Count);

            int ai = 1; CubicSignal.Vertex<TA> a = avs[0];
            int bi = 1; CubicSignal.Vertex<TB> b = bvs[0];
            while (true)
            {
                if (a.Time == b.Time)
                {
                    vs.Add(Function(a, b));
                    if (ai < avs.Count && bi < bvs.Count)
                    {
                        a = avs[ai];
                        b = bvs[ai];
                    }
                    else
                    {
                        break;
                    }
                    ai++;
                    bi++;
                    continue;
                }
                if (a.Time < b.Time)
                {
                    vs.Add(Function(a, B.GetVertex(a.Time)));
                    if (ai < avs.Count)
                    {
                        a = avs[ai];
                    }
                    else
                    {
                        break;
                    }
                    ai++;
                    continue;
                }
                if (b.Time < a.Time)
                {
                    vs.Add(Function(A.GetVertex(b.Time), b));
                    if (bi < bvs.Count)
                    {
                        b = bvs[bi];
                    }
                    else
                    {
                        break;
                    }
                    bi++;
                    continue;
                }
            }

            return new CubicSignal<T, TContinuum>(vs, Continuum);
        }

        /// <summary>
        /// Performs a unary operation on a signal.
        /// </summary>
        public static CubicSignal<T, TContinuum> UnaryOperation<TA, TAContinuum>(
            CubicSignal<TA, TAContinuum> A,
            Func<CubicSignal.Vertex<TA>, CubicSignal.Vertex<T>> Function,
            TContinuum Continuum)
            where TAContinuum : IContinuum<TA>
        {
            List<CubicSignal.Vertex<TA>> avs = A._Vertices;
            List<CubicSignal.Vertex<T>> vs = new List<CubicSignal.Vertex<T>>(avs.Count);
            foreach (CubicSignal.Vertex<TA> v in avs)
            {
                vs.Add(Function(v));
            }
            return new CubicSignal<T, TContinuum>(vs, Continuum);
        }

        /// <summary>
        /// Gets the product of two vertices occuring at the same time.
        /// </summary>
        public static CubicSignal.Vertex<T> Product<TA, TB, TMultiplication>(
            CubicSignal.Vertex<TA> A,
            CubicSignal.Vertex<TB> B,
            TMultiplication Multiplication,
            TContinuum Continuum)
            where TMultiplication : IMultiplication<TA, TB, T>
        {
            return new CubicSignal.Vertex<T>(
                A.Time,
                Multiplication.Multiply(A.Value, B.Value),
                Continuum.Add(
                    Multiplication.Multiply(A.Derivative, B.Value),
                    Multiplication.Multiply(A.Value, B.Derivative)));
        }

        /// <summary>
        /// Gets the vertices for this signal, in the order they occur at.
        /// </summary>
        public List<CubicSignal.Vertex<T>> Vertices
        {
            get
            {
                return this._Vertices;
            }
        }

        /// <summary>
        /// Gets the index of a vertex such that Time is in the interval [Vertices[index], Vertices[index + 1]), along with the corresponding
        /// vertices (named start and end) and other information needed to compute the spline.
        /// </summary>
        /// <param name="Delta">The length of the interval.</param>
        /// <param name="Param">The relative position of Time in the interval between 0.0 and 1.0.</param>
        public int GetInterval(double Time, out CubicSignal.Vertex<T> Start, out CubicSignal.Vertex<T> End, out double Delta, out double Param)
        {
            int l = 0;
            int h = this._Vertices.Count;
            while (h > l + 1)
            {
                int s = (l + h) / 2;
                if (Time >= this._Vertices[s].Time)
                {
                    l = s;
                }
                else
                {
                    h = s;
                }
            }

            Start = this._Vertices[l];
            End = this._Vertices[l + 1];
            Delta = End.Time - Start.Time;
            Param = (Time - Start.Time) / Delta;

            return l;
        }

        private TContinuum _Continuum;
        private List<CubicSignal.Vertex<T>> _Vertices;
    }
}