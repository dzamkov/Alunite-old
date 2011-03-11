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
        /// <summary>
        /// Creates an entity with a transform to this entity.
        /// </summary>
        public virtual TransformedEntity Apply(Transform Transform)
        {
            return new TransformedEntity(this, Transform);
        }

        /// <summary>
        /// Gets a camera entity.
        /// </summary>
        public static CameraEntity Camera()
        {
            return CameraEntity.Singleton;
        }

        /// <summary>
        /// Creates a sphere with the specified mass and radius.
        /// </summary>
        public static SphereEntity Sphere(double Mass, double Radius)
        {
            return new SphereEntity(Mass, Radius);
        }
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

        /// <summary>
        /// Gets the phantom entity that is "embodied".
        /// </summary>
        public PhantomEntity Phantom
        {
            get
            {
                return this._Phantom;
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

        private PhantomEntity _Phantom;
        private Entity _Body;
    }

    /// <summary>
    /// An entity that represents a transformed form of a source entity.
    /// </summary>
    public class TransformedEntity : Entity
    {
        public TransformedEntity(Entity Source, Transform Transform)
        {
            this._Source = Source;
            this._Transform = Transform;
        }

        public override TransformedEntity Apply(Transform Transform)
        {
            return new TransformedEntity(this._Source, this._Transform.Apply(Transform));
        }

        /// <summary>
        /// Gets the entity that is transformed.
        /// </summary>
        public Entity Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the transform used.
        /// </summary>
        public Transform Transform
        {
            get
            {
                return this._Transform;
            }
        }

        private Entity _Source;
        private Transform _Transform;
    }
}