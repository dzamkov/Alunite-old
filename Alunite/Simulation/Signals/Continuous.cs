using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// The generalized base class of continuous signals.
    /// </summary>
    public abstract class ContinuousSignal<T> : Signal<T>
    {

    }

    /// <summary>
    /// A signal of continuous values over a continuum (number line).
    /// </summary>
    public abstract class ContinuousSignal<T, TContinuum> : ContinuousSignal<T>
        where TContinuum : IContinuum<T>
    {
        /// <summary>
        /// Gets the continuum, or number line, this signal uses.
        /// </summary>
        public abstract TContinuum Continuum { get; }

        /// <summary>
        /// Gets the derivative of this signal at the specified time.
        /// </summary>
        public virtual T GetDerivative(double Time)
        {
            const double h = 0.001;
            TContinuum ct = this.Continuum;
            return ct.Multiply(ct.Subtract(this[Time + h], this[Time]), 1.0 / h);
        }
    }
}