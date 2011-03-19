using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A span for a stable material entity simulated with no internal interactions as a single solid piece of matter.
    /// </summary>
    public abstract class RigidBodySpan : Span
    {
        public override Entity this[double Time]
        {
            get 
            {
                return this.Entity.Apply(this.Transform[Time]);
            }
        }

        public override Entity Initial
        {
            get
            {
                return this.Entity;
            }
        }

        /// <summary>
        /// Gets the entity for the rigid body. Note that this is the same entity used at all times throughout the span.
        /// </summary>
        public abstract Entity Entity { get; }

        /// <summary>
        /// Gets a signal showing the progression of the transform from entity to span coordinates over time.
        /// </summary>
        public abstract Signal<Transform> Transform { get; }
    }
}