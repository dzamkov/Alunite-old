using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Represents an untransformed object that can participate in physical interactions. Note that all operations
    /// on matter use units of meters, seconds, kilograms and kelvin.
    /// </summary>
    public abstract class Matter
    {
        private class _NullMatter : Matter
        {
            public override Matter Update(Matter Environment, double Time)
            {
                return this;
            }
        }

        /// <summary>
        /// Gets a matter that has no effects or interactions.
        /// </summary>
        public static readonly Matter Null = new _NullMatter();

        /// <summary>
        /// Creates the updated form of this matter given the environment (which is all matter in the world excluding, and given 
        /// in the frame of reference of the matter in question) by a given amount of time in seconds.
        /// </summary>
        public abstract Matter Update(Matter Environment, double Time);

        /// <summary>
        /// Gets the gravitational force that would act on an object of the given mass and at the specified offset from this matter.
        /// </summary>
        public virtual Vector GetGravityForce(Vector Position, double Mass)
        {
            Vector force = new Vector(0.0, 0.0, 0.0);
            foreach (Particle p in this.Particles)
            {
                Vector to = p.Position - Position;
                double dis = to.Length;
                force += to * (Matter.G * (p.Mass + Mass) / (dis * dis * dis));
            }
            return force;
        }

        /// <summary>
        /// Gets particles in this matter within a certain distance of the given position.
        /// </summary>
        public virtual IEnumerable<Particle> GetParticles(Vector Position, double Distance)
        {
            double dd = Distance * Distance;
            return
                from p in this.Particles
                where (p.Position - Position).SquareLength <= dd
                select p;
        }

        /// <summary>
        /// The gravitational constant which satisfies F = G * (M1 + M2) / (R * R) with F being the force of gravity in newtons, M1 and M2 the mass of the objects
        /// in kilograms and R the distance between them in meters.
        /// </summary>
        public static double G = 6.67428e-11;

        /// <summary>
        /// Gets the particles in this matter.
        /// </summary>
        public virtual IEnumerable<Particle> Particles
        {
            get
            {
                return new Particle[0];
            }
        }

        /// <summary>
        /// Applies a transform to this matter.
        /// </summary>
        public virtual Matter Apply(Transform Transform)
        {
            return new TransformMatter(this, Transform);
        }
    }

    /// <summary>
    /// Matter made by a physical composition of other matter.
    /// </summary>
    public abstract class CompositeMatter : Matter
    {
        /// <summary>
        /// Gets the pieces of matter that makes up this matter. The order should not matter (lol).
        /// </summary>
        public abstract IEnumerable<Matter> Elements { get; }

        public override Vector GetGravityForce(Vector Position, double Mass)
        {
            Vector force = new Vector(0.0, 0.0, 0.0);
            foreach (Matter e in this.Elements)
            {
                force += e.GetGravityForce(Position, Mass);
            }
            return force;
        }

        /// <summary>
        /// Creates composite matter from the specified elements.
        /// </summary>
        public static CompositeMatter Create(IEnumerable<Matter> Elements)
        {
            return new _Concrete(Elements);
        }

        private class _Concrete : CompositeMatter
        {
            public _Concrete(IEnumerable<Matter> Elements)
            {
                this._Elements = Elements;
            }

            public override IEnumerable<Matter> Elements
            {
                get
                {
                    return this._Elements;
                }
            }

            private IEnumerable<Matter> _Elements;
        }

        public override Matter Update(Matter Environment, double Time)
        {
            LinkedList<Matter> elems = new LinkedList<Matter>(this.Elements);
            List<Matter> res = new List<Matter>(elems.Count);

            LinkedListNode<Matter> cur = elems.First;
            elems.AddFirst(Environment);

            while (cur != null)
            {
                Matter curmat = cur.Value;
                LinkedListNode<Matter> next = cur.Next;
                elems.Remove(cur);

                res.Add(curmat.Update(Create(elems), Time));
                elems.AddFirst(curmat);
                cur = next;
            }
            if (res.Count == 0)
            {
                return Matter.Null;
            }
            if (res.Count == 1)
            {
                return res[0];
            }
            return Create(res);
        }

        public override IEnumerable<Particle> Particles
        {
            get
            {
                return
                    from e in this.Elements
                    from p in e.Particles
                    select p;
            }
        }
    }

    /// <summary>
    /// Represents a possible the orientation, translation and velocity offset for matter.
    /// </summary>
    public struct Transform
    {
        public Transform(Vector Offset, Vector VelocityOffset, Quaternion Rotation)
        {
            this.Offset = Offset;
            this.VelocityOffset = VelocityOffset;
            this.Rotation = Rotation;
        }

        /// <summary>
        /// Applies this transform to another, in effect combining them.
        /// </summary>
        public Transform ApplyTo(Transform Transform)
        {
            return new Transform(
                this.Offset + this.Rotation.Rotate(Transform.Offset),
                this.VelocityOffset + this.Rotation.Rotate(Transform.VelocityOffset),
                this.Rotation * Transform.Rotation);
        }

        /// <summary>
        /// Gets the inverse of this transform.
        /// </summary>
        public Transform Inverse
        {
            get
            {
                Quaternion nrot = this.Rotation.Conjugate;
                return new Transform(
                    nrot.Rotate(-this.Offset),
                    nrot.Rotate(-this.VelocityOffset),
                    nrot);
            }
        }

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        public static Transform Identity
        {
            get
            {
                return new Transform(
                    new Vector(0.0, 0.0, 0.0), 
                    new Vector(0.0, 0.0, 0.0), 
                    Quaternion.Identity);
            }
        }

        public Vector Offset;
        public Vector VelocityOffset;
        public Quaternion Rotation;
    }

    /// <summary>
    /// A piece of transformed matter.
    /// </summary>
    public class TransformMatter : Matter
    {
        public TransformMatter(Matter Source, Transform Transform)
        {
            this._Source = Source;
            this._Transform = Transform;
        }

        /// <summary>
        /// Gets the transform of the source matter.
        /// </summary>
        public Transform Transform
        {
            get
            {
                return this._Transform;
            }
        }

        /// <summary>
        /// Gets the source matter .
        /// </summary>
        public Matter Source
        {
            get
            {
                return this._Source;
            }
        }

        public override Matter Update(Matter Environment, double Time)
        {
            Transform ntrans = this._Transform;
            ntrans.Offset += ntrans.VelocityOffset * Time;
            return this._Source.Update(Environment.Apply(this._Transform.Inverse), Time).Apply(ntrans);
        }

        public override Matter Apply(Transform Transform)
        {
            return new TransformMatter(this._Source, Transform.ApplyTo(this._Transform));
        }

        public override IEnumerable<Particle> Particles
        {
            get
            {
                return
                    from p in this._Source.Particles
                    select p.Apply(this._Transform);
            }
        }

        private Transform _Transform;
        private Matter _Source;
    }
}