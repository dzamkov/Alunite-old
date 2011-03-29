using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A span for a span entity.
    /// </summary>
    public class SignalerSpan<T> : Span
    {
        public SignalerSpan(Signal<T> Signal, OutTerminal<T> Terminal)
        {
            this._Signal = Signal;
            this._Terminal = Terminal;
        }

        /// <summary>
        /// Gets the signal that is outputted.
        /// </summary>
        public Signal<T> Signal
        {
            get
            {
                return this._Signal;
            }
        }

        /// <summary>
        /// Gets the terminal the signal is outputted to.
        /// </summary>
        public OutTerminal<T> Terminal
        {
            get
            {
                return this._Terminal;
            }
        }

        public override Entity this[double Time]
        {
            get 
            {
                return new SignalerEntity<T>(this._Signal.Advance(Time), this._Terminal);
            }
        }

        public override Entity Initial
        {
            get
            {
                return new SignalerEntity<T>(this._Signal, this._Terminal);
            }
        }

        public override Signal<Maybe<F>> Read<F>(OutTerminal<F> Terminal)
        {
            if ((Node)Terminal == (Node)this._Terminal)
            {
                return Alunite.Signal.Just((Signal<F>)(object)this._Signal);
            }
            else
            {
                return Alunite.Signal.Nothing<F>();
            }
        }

        public override Span Apply(Transform Transform)
        {
            return this;
        }

        public override Span Apply(Signal<Transform> Path)
        {
            return this;
        }

        public override Span Update(Span Environment, ControlInput Input)
        {
            return this;
        }

        private Signal<T> _Signal;
        private OutTerminal<T> _Terminal;
    }
}