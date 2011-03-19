using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A span defined by a single static entity and the terminals it uses. The entity defined by this span has no
    /// state information.
    /// </summary>
    public abstract class StaticSpan : Span
    {
        public StaticSpan(Entity Entity)
        {
            this._Entity = Entity;
        }

        /// <summary>
        /// Creates a static span with no exposed terminals for the specified entity.
        /// </summary>
        public static SimpleStaticSpan CreateSimple(Entity Entity)
        {
            return new SimpleStaticSpan(Entity);
        }

        /// <summary>
        /// Gets the entity that defines the state of this span at any one time.
        /// </summary>
        public Entity Entity
        {
            get
            {
                return this._Entity;
            }
        }

        public override Entity Initial
        {
            get
            {
                return this._Entity;
            }
        }

        public override Entity this[double Time]
        {
            get
            {
                return this._Entity;
            }
        }

        private Entity _Entity;
    }

    /// <summary>
    /// A static span with no exposed nodes.
    /// </summary>
    public sealed class SimpleStaticSpan : StaticSpan
    {
        public SimpleStaticSpan(Entity Entity)
            : base(Entity)
        {

        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            return Signal.Nothing<T>();    
        }

        public override Span Apply(Transform Transform)
        {
            return new SimpleStaticSpan(this.Entity.Apply(Transform));
        }
    }
}