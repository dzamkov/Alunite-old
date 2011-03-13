using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity that creates internal links within another source entity.
    /// </summary>
    public class LinkEntity : Entity
    {
        public LinkEntity(Entity Source)
        {
            this._Source = Source;
            this._Links = new List<_Link>();
        }

        /// <summary>
        /// Gets the source entity for this link entity.
        /// </summary>
        public Entity Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Links two terminals within the source entity. Note that once a terminal is linked, it can no longer
        /// be used outside the entity.
        /// </summary>
        public void Link<TInput, TOutput>(Terminal<TInput, TOutput> A, Terminal<TOutput, TInput> B)
        {
            this._Links.Add(_Link.Create<TInput, TOutput>(A, B));
        }

        /// <summary>
        /// Links two terminals within the source entity. Note that once a terminal is linked, it can no longer
        /// be used outside the entity.
        /// </summary>
        public void Link<TOutput>(Terminal<Void, TOutput> Output, Terminal<TOutput, Void> Input)
        {
            this._Links.Add(_Link.Create<Void, TOutput>(Output, Input));
        }

        public override bool Phantom
        {
            get
            {
                return this._Source.Phantom;
            }
        }

        private Entity _Source;
        private List<_Link> _Links;
    }

    /// <summary>
    /// A link within a link entity.
    /// </summary>
    internal class _Link
    {
        /// <summary>
        /// Creates a link between two terminals.
        /// </summary>
        public static _Link Create<TA, TB>(Terminal<TA, TB> A, Terminal<TB, TA> B)
        {
            return new SpecialTerminal<TA, TB>()
            {
                A = A,
                B = B
            };
        }

        /// <summary>
        /// A specialized link between complimentary typed terminals.
        /// </summary>>
        public class SpecialTerminal<TA, TB> : _Link
        {
            public Terminal<TA, TB> A;
            public Terminal<TB, TA> B;
        }
    }
}