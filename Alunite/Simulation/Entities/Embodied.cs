using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity which attaches a phantom entity to a physical form. This allows the attached entity to move and be destroyed with the physical entity while still retaining
    /// its special properties. If any node reference is defined in both the control and the body, it has priority towards the control.
    /// </summary>
    /// <remarks>The reason I can't call control something sensible like "Phantom" is because that is already a property of "Entity".</remarks>
    public class EmbodiedEntity : Entity
    {
        public EmbodiedEntity(Entity Control, Entity Body)
        {
            this._Control = Control;
            this._Body = Body;
        }

        /// <summary>
        /// Gets the phantom entity that is "embodied".
        /// </summary>
        public Entity Control
        {
            get
            {
                return this._Control;
            }
        }

        /// <summary>
        /// Gets the physical entity used as the body.
        /// </summary>
        public Entity Body
        {
            get
            {
                return this._Body;
            }
        }

        public override MassAggregate Aggregate
        {
            get
            {
                return this._Body.Aggregate;
            }
        }

        public override bool Phantom
        {
            get
            {
                return false;
            }
        }

        private Entity _Control;
        private Entity _Body;
    }
}