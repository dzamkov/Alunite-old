using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A static description of a dynamic object that can interact with other entities in a simulation over time. Note that each entity may be used multiple
    /// times and is not linked with any particular context.
    /// </summary>
    public abstract class Entity : Data<Entity>
    {
        /// <summary>
        /// Creates an entity with a transform to this entity.
        /// </summary>
        public virtual Entity Apply(Transform Transform)
        {
            return new TransformedEntity(this, Transform);
        }

        /// <summary>
        /// Gets wether or not this entity has any physical components. A phantom entity is invisible, massless and indescructible.
        /// </summary>
        public abstract bool Phantom { get; }

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
        /// Creates a superimposed compound of entities. If any single node reference is used with both entities, the node reference
        /// will have priority towards the primary entity.
        /// </summary>
        public static Entity Combine(Entity Primary, Entity Secondary)
        {
            if (Primary == Null)
            {
                return Secondary;
            }
            if (Secondary == Null)
            {
                return Primary;
            }
            return new BinaryEntity(Primary, Secondary);
        }

        /// <summary>
        /// Creates a superimposed compound of entities. If any single node reference is used more than once throughout the given entities, the
        /// node reference will have priority towards the begining of the collection.
        /// </summary>
        public static Entity Combine(IEnumerable<Entity> Entities)
        {
            Entity cur = Null;
            foreach (Entity e in Entities)
            {
                if (cur == Null)
                {
                    cur = e;
                }
                else
                {
                    if (e != Null)
                    {
                        cur = new BinaryEntity(cur, e);
                    }
                }
            }
            return cur;
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
        /// Gets the null entity. This entity has no interactions and can be completely disregarded.
        /// </summary>
        public static NullEntity Null
        {
            get
            {
                return NullEntity.Singleton;
            }
        }

        /// <summary>
        /// Creates an entity that attaches this entity to the specified physical body. Only phantom entities may be embodied.
        /// </summary>
        public EmbodiedEntity Embody(Entity Body)
        {
            return new EmbodiedEntity(this, Body);
        }
    }

    /// <summary>
    /// An undetectable entity with no interactions.
    /// </summary>
    public class NullEntity : Entity
    {
        private NullEntity()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly NullEntity Singleton = new NullEntity();

        public override MassAggregate Aggregate
        {
            get
            {
                return MassAggregate.Null;
            }
        }

        public override Entity Apply(Transform Transform)
        {
            return this;
        }

        public override bool Phantom
        {
            get
            {
                return false;
            }
        }
    }

    /// <summary>
    /// An invisible, indestructible, massless entity that may interact with a simulation.
    /// </summary>
    public abstract class PhantomEntity : Entity
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
    /// An entity that represents a transformed form of a source entity.
    /// </summary>
    public class TransformedEntity : Entity
    {
        public TransformedEntity(Entity Source, Transform Transform)
        {
            this._Source = Source;
            this._Transform = Transform;
        }

        public override Entity Apply(Transform Transform)
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