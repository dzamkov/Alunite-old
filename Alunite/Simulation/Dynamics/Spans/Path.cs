using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Transforms a source span by a path (a signal of transforms).
    /// </summary>
    public class PathSpan : Span
    {
        public PathSpan(Span Source, Signal<Transform> Path)
        {
            this._Source = Source;
            this._Path = Path;
        }

        public override Entity this[double Time]
        {
            get
            {
                return this._Source[Time].Apply(this._Path[Time]);
            }
        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            return this._Source.Read(Terminal);
        }

        private Signal<Transform> _Path;
        private Span _Source;
    }
}