using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A signal of continuous values over a continuum (number line).
    /// </summary>
    public abstract class ContinuousSignal<T, TContinuum> : Signal<T>
        where TContinuum : IContinuum<T>
    {
        /// <summary>
        /// Gets the continuum, or number line, this signal uses.
        /// </summary>
        public abstract TContinuum Continuum { get; }

        /// <summary>
        /// Gets a signal that represents the derivative of this signal.
        /// </summary>
        public virtual ContinuousSignal<T, TContinuum> Derivative
        {
            get
            {
                return new DerivativeSignal<T, TContinuum>(this, Continuum);
            }
        }
    }

    /// <summary>
    /// A sum of two continuous signals.
    /// </summary>
    public class SumSignal<T, TContinuum> : ContinuousSignal<T, TContinuum>
        where TContinuum : IContinuum<T>
    {
        public SumSignal(Signal<T> A, Signal<T> B, TContinuum Continuum)
        {
            this._Continuum = Continuum;
            this._A = A;
            this._B = B;
        }

        /// <summary>
        /// Gets one of the components of the sum.
        /// </summary>
        public Signal<T> A
        {
            get
            {
                return this._A;
            }
        }

        /// <summary>
        /// Gets one of the components of the sum.
        /// </summary>
        public Signal<T> B
        {
            get
            {
                return this._B;
            }
        }

        public override TContinuum Continuum
        {
            get
            {
                return this._Continuum;
            }
        }

        public override T this[double Time]
        {
            get
            {
                return this._Continuum.Add(this._A[Time], this._B[Time]);
            }
        }

        public override double Length
        {
            get
            {
                return Math.Min(this._A.Length, this._B.Length);
            }
        }

        private TContinuum _Continuum;
        private Signal<T> _A;
        private Signal<T> _B;
    }

    /// <summary>
    /// A difference of two continuous signals.
    /// </summary>
    public class DifferenceSignal<T, TContinuum> : ContinuousSignal<T, TContinuum>
        where TContinuum : IContinuum<T>
    {
        public DifferenceSignal(Signal<T> A, Signal<T> B, TContinuum Continuum)
        {
            this._Continuum = Continuum;
            this._A = A;
            this._B = B;
        }

        /// <summary>
        /// Gets the minuend of the difference.
        /// </summary>
        public Signal<T> A
        {
            get
            {
                return this._A;
            }
        }

        /// <summary>
        /// Gets the subtrahend of the difference.
        /// </summary>
        public Signal<T> B
        {
            get
            {
                return this._B;
            }
        }

        public override TContinuum Continuum
        {
            get
            {
                return this._Continuum;
            }
        }

        public override T this[double Time]
        {
            get
            {
                return this._Continuum.Subtract(this._A[Time], this._B[Time]);
            }
        }

        public override double Length
        {
            get
            {
                return Math.Min(this._A.Length, this._B.Length);
            }
        }

        private TContinuum _Continuum;
        private Signal<T> _A;
        private Signal<T> _B;
    }
}