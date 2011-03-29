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
        /// Creates a span for this entity.
        /// </summary>
        public abstract Span CreateSpan(Span Environment, ControlInput Input);

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
        public static Brush Brush(Shape<Substance> Shape)
        {
            return new Brush(Shape);
        }

        /// <summary>
        /// Creates a solid brush.
        /// </summary>
        public static Brush Brush(Substance Substance, Mask Mask)
        {
            return new Brush(Mask.Map<Substance>(x => x ? Substance : Substance.Vacuum));
        }

        /// <summary>
        /// Creates a link between terminals on a source entity.
        /// </summary>
        public static TerminalLinkEntity<T> Link<T>(Entity Source, OutTerminal<T> Output, InTerminal<T> Input)
        {
            return new TerminalLinkEntity<T>(Source, Output, Input);
        }

        /// <summary>
        /// Creates a new entity builder.
        /// </summary>
        public static EntityBuilder Builder()
        {
            return new EntityBuilder();
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

        public override Span CreateSpan(Span Environment, ControlInput Input)
        {
            return NullSpan.Singleton;
        }

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

        public override Span CreateSpan(Span Environment, ControlInput Input)
        {
            return this._Source.CreateSpan(Environment.Apply(this._Transform.Inverse), Input).Apply(this._Transform);
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

    /// <summary>
    /// Constructs an entity from various elements.
    /// </summary>
    public class EntityBuilder : Builder<Entity>
    {
        public EntityBuilder()
        {
            this._Current = Entity.Null;
        }

        /// <summary>
        /// Adds (superimposes) an entity on to this builders current entity.
        /// </summary>
        public void Add(Entity Entity)
        {
            this._Current = Entity.Combine(Entity, this._Current);
        }

        /// <summary>
        /// Adds an entity (given by its current builder state) to this builders current entity.
        /// </summary>
        public void Add(EntityBuilder Entity)
        {
            this.Add(Entity._Current);
        }

        /// <summary>
        /// Attaches an unbound signal to an input of this entity.
        /// </summary>
        public void Attach<T>(InTerminal<T> Input, Signal<T> Signal)
        {
            SignalerEntity<T> se = new SignalerEntity<T>(Signal);
            this.Add(se);
            this.Link<T>(se.Terminal, Input);
        }

        /// <summary>
        /// Links two complimentary terminals within the entity.
        /// </summary>
        public void Link<T>(OutTerminal<T> Output, InTerminal<T> Input)
        {
            this._Current = Entity.Link<T>(this._Current, Output, Input);
        }

        /// <summary>
        /// Applies a transform to this builders current entity.
        /// </summary>
        public void Apply(Transform Transform)
        {
            this._Current = this._Current.Apply(Transform);
        }

        /// <summary>
        /// Embodys this builders current entity into another entity. This may only be done if the current entity is a phantom entity.
        /// </summary>
        public void Embody(Entity Body)
        {
            this._Current = this._Current.Embody(Body);
        }

        public override Entity Finish()
        {
            return this._Current;
        }

        private Entity _Current;
    }
}