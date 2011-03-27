using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A general base class for all cubic signals of a certain type.
    /// </summary>
    public abstract class CubicSignal<T> : Signal<T>
    {
        /// <summary>
        /// A vertex in the cubic signal.
        /// </summary>
        public struct Vertex
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
    public class CubicSignal<T, TContinuum> : CubicSignal<T>
        where TContinuum : IContinuum<T>
    {
        public CubicSignal(Vertex[] Vertices, TContinuum Continuum)
        {
            this._Vertices = Vertices;
            this._Continuum = Continuum;
        }

        public override T this[double Time]
        {
            get
            {
                int i = this.GetInterval(Time);
                Vertex f = this._Vertices[i];
                Vertex s = this._Vertices[i + 1];
                double delta = s.Time - f.Time;
                double param = (Time - f.Time) / delta;

                TContinuum ct = this._Continuum;

                T va = f.Value;
                T vb = ct.Add(f.Value, ct.Multiply(f.Derivative, delta));
                T vc = ct.Subtract(s.Value, ct.Multiply(s.Derivative, delta));
                T vd = s.Value;

                T ve = ct.Mix(va, vb, param);
                T vf = ct.Mix(vb, vc, param);
                T vg = ct.Mix(vc, vd, param);

                T vh = ct.Mix(ve, vf, param);
                T vi = ct.Mix(vf, vg, param);

                return ct.Mix(vh, vi, param);
            }
        }

        /// <summary>
        /// Gets the derivative of this signal at the specified time.
        /// </summary>
        public T GetDerivative(double Time)
        {
            int i = this.GetInterval(Time);
            Vertex f = this._Vertices[i];
            Vertex s = this._Vertices[i + 1];
            double delta = s.Time - f.Time;
            double param = (Time - f.Time) / delta;

            TContinuum ct = this._Continuum;

            T va = f.Derivative;
            T vb = ct.Multiply(ct.Subtract(s.Value, f.Value), 3.0 / delta);
            T vc = s.Derivative;

            T vd = ct.Mix(va, vb, param);
            T ve = ct.Mix(vb, vc, param);

            return ct.Mix(vd, ve, param);
        }

        public override double Length
        {
            get
            {
                return this._Vertices[this._Vertices.Length - 1].Time;
            }
        }

        /// <summary>
        /// Creates a cubic signal that varies linearly between two values for the given amount of time.
        /// </summary>
        public static CubicSignal<T, TContinuum> Linear(TContinuum Continuum, T First, T Last, double Length)
        {
            double ilen = 1.0 / Length;
            T d = Continuum.Multiply(Continuum.Subtract(Last, First), ilen);
            return new CubicSignal<T, TContinuum>(new Vertex[]
            {
                new Vertex(0.0, First, d),
                new Vertex(Length, Last, d)
            }, Continuum);
        }

        /// <summary>
        /// Gets the vertices for this signal, in the order they occur at.
        /// </summary>
        public Vertex[] Vertices
        {
            get
            {
                return this._Vertices;
            }
        }

        /// <summary>
        /// Gets the index of a vertex such that Time is in the interval [Vertices[index], Vertices[index + 1]).
        /// </summary>
        public int GetInterval(double Time)
        {
            int l = 0;
            int h = this._Vertices.Length;
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
            return l;
        }

        private TContinuum _Continuum;
        private Vertex[] _Vertices;
    }
}