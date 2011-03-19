using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity made from the superimposed combination of two other entities. 
    /// </summary>
    /// <remarks>
    /// This type of entity allows there
    /// to be multiple nodes with the same reference. If this is the case, the node from the primary portion is given priority. Use a remapping
    /// entity before the binary entity to allow for unambiguous node references.
    /// </remarks>
    public class BinaryEntity : Entity
    {
        public BinaryEntity(Entity Primary, Entity Secondary)
        {
            this._Primary = Primary;
            this._Secondary = Secondary;
        }

        /// <summary>
        /// Gets the primary entity in this compound. This entity is given priority with nodes.
        /// </summary>
        public Entity Primary
        {
            get
            {
                return this._Primary;
            }
        }

        /// <summary>
        /// Gets the secondary entity in this compound.
        /// </summary>
        public Entity Secondary
        {
            get
            {
                return this._Secondary;
            }
        }

        public override MassAggregate Aggregate
        {
            get
            {
                return this._Primary.Aggregate + this._Secondary.Aggregate;
            }
        }

        public override bool Phantom
        {
            get
            {
                return this._Primary.Phantom && this._Secondary.Phantom;
            }
        }

        private Entity _Primary;
        private Entity _Secondary;
    }

}