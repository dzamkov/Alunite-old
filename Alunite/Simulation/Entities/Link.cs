using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity that creates or manipulates an internal link between logical (or control) nodes within a source entity.
    /// </summary>
    public abstract class LinkEntity : Entity
    {
        public LinkEntity(Entity Source)
        {
            this._Source = Source;
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
        /// Creates a link entity which forms the same link this entity has on another entity.
        /// </summary>
        public abstract LinkEntity Relink(Entity Source);

        public override Entity Apply(Transform Transform)
        {
            return this.Relink(this._Source.Apply(Transform));
        }

        public override MassAggregate Aggregate
        {
            get
            {
                return this._Source.Aggregate;
            }
        }

        public override bool Phantom
        {
            get
            {
                return this._Source.Phantom;
            }
        }

        private Entity _Source;
    }

    /// <summary>
    /// An entity that creates an internal link between terminals within a source entity.
    /// </summary>
    public abstract class TerminalLinkEntity : LinkEntity
    {
        public TerminalLinkEntity(Entity Source)
            : base(Source)
        {

        }
    }

    /// <summary>
    /// A specialized terminal link entity.
    /// </summary>
    public class TerminalLinkEntity<T> : TerminalLinkEntity
    {
        public TerminalLinkEntity(Entity Source, OutTerminal<T> Output, InTerminal<T> Input)
            : base(Source)
        {
            this._Output = Output;
            this._Input = Input;
        }

        /// <summary>
        /// Gets the output terminal of the link.
        /// </summary>
        public OutTerminal<T> Output
        {
            get
            {
                return this._Output;
            }
        }

        /// <summary>
        /// Gets the input terminal of the link.
        /// </summary>
        public InTerminal<T> Input
        {
            get
            {
                return this._Input;
            }
        }

        public override LinkEntity Relink(Entity Source)
        {
            return new TerminalLinkEntity<T>(Source, this._Output, this._Input);
        }

        public override Span CreateSpan(Span Environment, ControlInput Input)
        {
            throw new NotImplementedException();
        }

        private OutTerminal<T> _Output;
        private InTerminal<T> _Input;
    }
}