using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A static description of a dynamic object that can interact with other entities in a simulation over time. Note that each entity may be used multiple
    /// times and is not linked with any particular context.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Creates an entity with a transform to this entity.
        /// </summary>
        public virtual TransformedEntity Apply(Transform Transform)
        {
            return new TransformedEntity(this, Transform);
        }

        /// <summary>
        /// Gets wether or not this entity has any physical components. A phantom entity is invisible, massless and indescructible.
        /// </summary>
        public virtual bool Phantom
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an aggregation of the physical contents of this entity.
        /// </summary>
        public abstract MassAggregate Aggregate { get; }

        /// <summary>
        /// Gets a camera entity.
        /// </summary>
        public static CameraEntity Camera()
        {
            return CameraEntity.Singleton;
        }

        /// <summary>
        /// Creates a compound entity.
        /// </summary>
        public static CompoundEntity Compound()
        {
            return new CompoundEntity();
        }

        /// <summary>
        /// Creates a brush.
        /// </summary>
        public static Brush Brush(Material Material, Shape Shape)
        {
            return new Brush(Material, Shape);
        }

        /// <summary>
        /// Creates a solid brush.
        /// </summary>
        public static Brush Brush(Substance Substance, Shape Shape)
        {
            return new Brush(Material.Solid(Substance), Shape);
        }

        /// <summary>
        /// Creates a linking entity on another entity.
        /// </summary>
        public static LinkEntity Link(Entity Source)
        {
            return new LinkEntity(Source);
        }

        /// <summary>
        /// Creates an entity that attaches this entity to the specified physical body. Only phantom entities may be embodied.
        /// </summary>
        public EmbodimentEntity Embody(Entity Body)
        {
            return new EmbodimentEntity(this, Body);
        }
    }

    /// <summary>
    /// An invisible, indestructible, massless entity that may interact with a simulation.
    /// </summary>
    public class PhantomEntity : Entity
    {
        public override MassAggregate Aggregate
        {
            get
            {
                return MassAggregate.Null;
            }
        }

        public override bool Phantom
        {
            get
            {
                return true;
            }
        }
    }

    /// <summary>
    /// An entity which attaches a phantom entity to a physical form. This allows the attached entity to move and be destroyed with the physical entity while still retaining
    /// its special properties.
    /// </summary>
    public class EmbodimentEntity : Entity
    {
        public EmbodimentEntity(Entity Control, Entity Body)
        {
            this._Control = Control;
            this._Body = Body;
            this._BodyMap = new LazyNodeMap();
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

        /// <summary>
        /// Gets the node map from terminals internal to the body to nodes that can be referenced externally on this entity. No
        /// mapping is required for the "Phantom" part of this entity.
        /// </summary>
        public NodeMap BodyMap
        {
            get
            {
                return this._BodyMap;
            }
        }

        private Entity _Control;
        private Entity _Body;
        private LazyNodeMap _BodyMap;
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

        public override MassAggregate Aggregate
        {
            get
            {
                MassAggregate source = this._Source.Aggregate;
                source.Barycenter = this._Transform.ApplyToOffset(source.Barycenter);
                return source;
            }
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

        public override bool Phantom
        {
            get
            {
                return this._Source.Phantom;
            }
        }

        private Entity _Source;
        private Transform _Transform;
    }

    /// <summary>
    /// Aggregation of the physical contents of an entity.
    /// </summary>
    public struct MassAggregate
    {
        public MassAggregate(double Mass, Vector Barycenter)
        {
            this.Mass = Mass;
            this.Barycenter = Barycenter;
        }

        /// <summary>
        /// Gets a mass aggregation for no matter.
        /// </summary>
        public static MassAggregate Null
        {
            get
            {
                return new MassAggregate(0.0, new Vector(0.0, 0.0, 0.0));
            }
        }

        public static MassAggregate operator +(MassAggregate A, MassAggregate B)
        {
            double totalmass = A.Mass + B.Mass;
            return new MassAggregate(totalmass, A.Barycenter * (A.Mass / totalmass) + B.Barycenter * (B.Mass / totalmass));
        }

        public static MassAggregate operator -(MassAggregate A, MassAggregate B)
        {
            double totalmass = A.Mass + B.Mass;
            return new MassAggregate(A.Mass - B.Mass, A.Barycenter * (A.Mass / totalmass) - B.Barycenter * (B.Mass / totalmass));
        }

        /// <summary>
        /// The total mass in kilograms.
        /// </summary>
        public double Mass;

        /// <summary>
        /// The location center of mass, or barycenter in relation to the entity.
        /// </summary>
        public Vector Barycenter;
    }
}