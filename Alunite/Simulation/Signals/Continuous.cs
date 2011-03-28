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
}