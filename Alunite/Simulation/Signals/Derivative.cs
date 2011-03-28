using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Represents the derivative of a source signal using a given continuum.
    /// </summary>
    public class DerivativeSignal<T, TContinuum> : ContinuousSignal<T, TContinuum>
        where TContinuum : IContinuum<T>
    {
        public DerivativeSignal(Signal<T> Source, TContinuum Continuum)
        {
            this._Source = Source;
            this._Continuum = Continuum;
        }

        public override TContinuum Continuum
        {
            get
            {
                return this._Continuum;
            }
        }

        public override double Length
        {
            get
            {
                return this._Source.Length;
            }
        }

        public override T this[double Time]
        {
            get
            {
                // Accuracy is not assured for method calls on data, so I can just go ahead and do this
                const double h = 0.01;
                TContinuum ct = this._Continuum;
                return ct.Multiply(ct.Subtract(this._Source[Time + h], this._Source[Time]), 1.0 / h);
            }
        }

        public override Signal<T> Simplify
        {
            get
            {
                ContinuousSignal<T, TContinuum> s = this._Source as ContinuousSignal<T, TContinuum>;
                if (s != null)
                {
                    ContinuousSignal<T, TContinuum> nd = s.Derivative;
                    if (!(nd is DerivativeSignal<T, TContinuum>))
                    {
                        return nd;
                    }
                }
                return this;
            }
        }

        private Signal<T> _Source;
        private TContinuum _Continuum;
    }
}