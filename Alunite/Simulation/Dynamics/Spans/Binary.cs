using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A span for a binary compound entity.
    /// </summary>
    public class BinarySpan : Span
    {
        public BinarySpan(Span Primary, Span Secondary)
        {
            this._Primary = Primary;
            this._Secondary = Secondary;
        }

        public override Entity this[double Time]
        {
            get
            {
                return Entity.Combine(this._Primary[Time], this._Secondary[Time]);
            }
        }

        public override Entity Initial
        {
            get
            {
                return new BinaryEntity(this._Primary.Initial, this._Secondary.Initial);
            }
        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            return Signal.Defer(this._Primary.Read(Terminal), this._Secondary.Read(Terminal));
        }

        private Span _Primary;
        private Span _Secondary;
    }
}