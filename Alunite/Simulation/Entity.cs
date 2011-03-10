using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A static description of a dynamic object that can interact with other entities in a simulation over time. Note that each entity may be used multiple
    /// times and is not linked with any particular context.
    /// </summary>
    public class Entity
    {

    }

    /// <summary>
    /// An invisible, indestructible, massless entity that may interact with a simulation.
    /// </summary>
    public class PhantomEntity : Entity
    {
        /// <summary>
        /// Creates an entity that gives this phantom entity the specified physical body.
        /// </summary>
        public EmbodimentEntity Embody(Entity Body)
        {
            return new EmbodimentEntity(this, Body);
        }
    }

    /// <summary>
    /// An entity which attaches a phantom entity a physical form. This allows the phantom entity to move and be destroyed like a physical entity while still retaining
    /// its special properties.
    /// </summary>
    public class EmbodimentEntity : Entity
    {
        public EmbodimentEntity(PhantomEntity Phantom, Entity Body)
        {
            this._Phantom = Phantom;
            this._Body = Body;
        }

        private PhantomEntity _Phantom;
        private Entity _Body;
    }
}