using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// A physics system where interactions between matter are memozied allowing for
    /// faster simulation.
    /// </summary>
    public class FastPhysics : IParticlePhysics<FastPhysicsMatter, FastPhysicsSubstance>
    {
        public FastPhysicsMatter Create(Particle<FastPhysicsSubstance> Particle)
        {
            return FastPhysicsMatter.Particle(this, Particle);
        }

        public FastPhysicsMatter Transform(FastPhysicsMatter Matter, Transform Transform)
        {
            return Matter.Apply(this, Transform);
        }

        public FastPhysicsMatter Update(FastPhysicsMatter Matter, FastPhysicsMatter Environment, double Time)
        {
            return Matter.Update(this, Environment, Time);
        }

        public FastPhysicsMatter Compose(IEnumerable<FastPhysicsMatter> Matter)
        {
            return new FastPhysicsMatter._SphereTree(this).Create(Matter);
        }

        public FastPhysicsMatter Null
        {
            get
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Matter in a fast physics system.
    /// </summary>
    public abstract class FastPhysicsMatter : IMatter
    {
        private FastPhysicsMatter()
        {
            this._Usages = new UsageSet<_Binary>();
        }

        /// <summary>
        /// Gets the bounding sphere for this matter. The bounding sphere encloses all mass within the matter and does not
        /// have any correlation to where the matter can apply force.
        /// </summary>
        public abstract void GetBoundingSphere(out Vector Position, out double Radius);

        /// <summary>
        /// Applies a transform to this matter.
        /// </summary>
        public virtual FastPhysicsMatter Apply(FastPhysics Physics, Transform Transform)
        {
            return new _Transformed()
            {
                Source = this,
                Transform = Transform
            };
        }

        /// <summary>
        /// Gets the updated form of this matter in the specified environment after the given time.
        /// </summary>
        public virtual FastPhysicsMatter Update(FastPhysics Physics, FastPhysicsMatter Environment, double Time)
        {
            return null;
        }

        /// <summary>
        /// Creates matter for a particle.
        /// </summary>
        public static FastPhysicsMatter Particle(FastPhysics Physics, Particle<FastPhysicsSubstance> Particle)
        {
            return new _Transformed()
            {
                Transform = new Transform(Particle.Position, Particle.Velocity, Particle.Orientation),
                Source = /*new _Particle()
                {
                    Mass = Particle.Mass,
                    Spin = Particle.Spin,
                    Substance = Particle.Substance
                }*/ _Particle.Default
            };
        }

        /// <summary>
        /// Combines two "bits" of matter into one.
        /// </summary>
        public static FastPhysicsMatter Combine(FastPhysics Physics, FastPhysicsMatter A, FastPhysicsMatter B)
        {
            // Untransform
            _Transformed atrans = A as _Transformed;
            _Transformed btrans = B as _Transformed;
            Transform atob = Transform.Identity;
            if (btrans != null)
            {
                atob = btrans.Transform;
                B = btrans.Source;
            }
            Transform? full = null;
            if (atrans != null)
            {
                Transform fullval = atrans.Transform;
                full = fullval = atrans.Transform;
                atob = atob.Apply(fullval.Inverse);
                A = atrans.Source;
            }

            // Create binary matter
            FastPhysicsMatter._Binary bin = new _Binary()
            {
                A = A,
                B = B,
                AToB = atob
            };
            Vector posa; double rada; A.GetBoundingSphere(out posa, out rada);
            Vector posb; double radb; B.GetBoundingSphere(out posb, out radb); posb = atob.ApplyToOffset(posb);
            SphereTree<FastPhysicsMatter>.Enclose(posa, rada, posb, radb, out bin.BoundCenter, out bin.BoundRadius);


            // Retransform (if needed)
            FastPhysicsMatter res = bin;
            if (full != null)
            {
                res = new _Transformed()
                {
                    Source = res,
                    Transform = full.Value
                };
            }
            return res;
        }

        /// <summary>
        /// A sphere tree for matter.
        /// </summary>
        internal class _SphereTree : SphereTree<FastPhysicsMatter>
        {
            public _SphereTree(FastPhysics Physics)
            {
                this._Physics = Physics;
            }

            public override FastPhysicsMatter CreateCompound(FastPhysicsMatter A, FastPhysicsMatter B)
            {
                return Combine(this._Physics, A, B);
            }

            public override void GetBound(FastPhysicsMatter Node, out Vector Position, out double Radius)
            {
                Node.GetBoundingSphere(out Position, out Radius);
            }

            public override bool GetSubnodes(FastPhysicsMatter Node, ref FastPhysicsMatter A, ref FastPhysicsMatter B)
            {
                return Node._GetSubnodes(this._Physics, ref A, ref B);
            }

            private FastPhysics _Physics;
        }

        /// <summary>
        /// Gets the subnodes for this node for use in a sphere tree.
        /// </summary>
        internal virtual bool _GetSubnodes(FastPhysics Physics, ref FastPhysicsMatter A, ref FastPhysicsMatter B)
        {
            return false;
        }

        /// <summary>
        /// Matter containing a single particle.
        /// </summary>
        internal class _Particle : FastPhysicsMatter
        {
            /// <summary>
            /// Default particle.
            /// </summary>
            public static readonly _Particle Default = new _Particle();

            public override void GetBoundingSphere(out Vector Position, out double Radius)
            {
                Position = new Vector(0.0, 0.0, 0.0);
                Radius = 0.0;
            }

            public ISubstance Substance;
            public double Mass;
            public Quaternion Spin;
        }

        /// <summary>
        /// Matter created by transforming some source matter.
        /// </summary>
        internal class _Transformed : FastPhysicsMatter
        {
            public override FastPhysicsMatter Apply(FastPhysics Physics, Transform Transform)
            {
                return new _Transformed()
                {
                    Source = this.Source,
                    Transform = Transform.ApplyTo(this.Transform)
                };
            }

            public override void GetBoundingSphere(out Vector Position, out double Radius)
            {
                Source.GetBoundingSphere(out Position, out Radius);
                Position = Transform.Offset + Transform.Rotation.Rotate(Position);
            }

            internal override bool _GetSubnodes(FastPhysics Physics, ref FastPhysicsMatter A, ref FastPhysicsMatter B)
            {
                if (this.Source._GetSubnodes(Physics, ref A, ref B))
                {
                    A = A.Apply(Physics, this.Transform);
                    B = B.Apply(Physics, this.Transform);
                    return true;
                }
                return false;
            }

            public FastPhysicsMatter Source;
            public Transform Transform;
        }

        /// <summary>
        /// Matter created by the combination of some untransformed matter and some
        /// transformed matter.
        /// </summary>
        internal class _Binary : FastPhysicsMatter
        {
            public override void GetBoundingSphere(out Vector Position, out double Radius)
            {
                Position = this.BoundCenter;
                Radius = this.BoundRadius;
            }

            internal override bool _GetSubnodes(FastPhysics Physics, ref FastPhysicsMatter A, ref FastPhysicsMatter B)
            {
                A = this.A;
                B = this.B.Apply(Physics, this.AToB);
                return true;
            }

            public Vector BoundCenter;
            public double BoundRadius;
            public FastPhysicsMatter A;
            public FastPhysicsMatter B;
            public Transform AToB;
        }

        private UsageSet<_Binary> _Usages;
    }

    /// <summary>
    /// A substance in a fast physics system.
    /// </summary>
    public class FastPhysicsSubstance : IAutoSubstance<FastPhysics, FastPhysicsMatter, FastPhysicsSubstance>
    {
        private FastPhysicsSubstance()
        {
            this._Usages = new UsageSet<FastPhysicsMatter._Particle>();
        }

        /// <summary>
        /// The default (and currently only) possible substance.
        /// </summary>
        public static readonly FastPhysicsSubstance Default = new FastPhysicsSubstance();

        public void Update(FastPhysics Physics, FastPhysicsMatter Environment, double Time, ref Particle<FastPhysicsSubstance> Particle)
        {
            Particle.Update(Time);
        }

        private UsageSet<FastPhysicsMatter._Particle> _Usages;
    }
}