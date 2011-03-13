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
        /// Creates a link between two terminals. There is a many to one relationship between output and
        /// input terminals.
        /// </summary>
        public void Link<T>(OutTerminal<T> Output, InTerminal<T> Input)
        {
            this._Links.Add(_Link.Create(Output, Input));
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
        public static _Link Create<T>(OutTerminal<T> Output, InTerminal<T> Input)
        {
            return new SpecialTerminal<T>
            {
                Output = Output,
                Input = Input
            };
        }

        /// <summary>
        /// A specialized link between complimentary typed terminals.
        /// </summary>>
        public class SpecialTerminal<T> : _Link
        {
            public OutTerminal<T> Output;
            public InTerminal<T> Input;
        }
    }
}