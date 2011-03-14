using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A span for a stable material entity simulated with no internal interactions as a single solid piece of matter.
    /// </summary>
    public class RigidBodySpan : Span
    {
        /// <summary>
        /// Gets the entity for the rigid body. Note that this is the same entity used at all times throughout the span.
        /// </summary>
        public Entity Entity
        {
            get
            {
                return this._Entity;
            }
        }

        private Entity _Entity;
    }
}