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
            throw new NotImplementedException();
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
        /// Estimates the complexity of this matter by getting the amount of unique bits of matter used to
        /// describe it.
        /// </summary>
        public int Complexity
        {
            get
            {
                HashSet<FastPhysicsMatter> used = new HashSet<FastPhysicsMatter>();
                this._GetUsed(used);
                return used.Count;
            }
        }

        /// <summary>
        /// Gets the position of all the particles in this matter, for debug purposes.
        /// </summary>
        public IEnumerable<Vector> Particles
        {
            get
            {
                List<Vector> parts = new List<Vector>();
                this._GetParticles(Transform.Identity, parts);
                return parts;
            }
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
        /// Quickly creates a lattice with a power of two size of the specified object.
        /// </summary>
        public static FastPhysicsMatter CreateLattice(FastPhysics Physics, int LogSize, FastPhysicsMatter Object, double Spacing)
        {
            // Get "major" spacing
            Spacing = Spacing * Math.Pow(2.0, LogSize - 1);

            // Extract transform
            _Transformed trans = Object as _Transformed;
            if (trans != null)
            {
                Object = trans.Source;
                return _CreateLattice(Physics, LogSize, Object, Spacing).Apply(Physics, trans.Transform);
            }
            else
            {
                return _CreateLattice(Physics, LogSize, Object, Spacing);
            }
        }

        private static FastPhysicsMatter _CreateLattice(FastPhysics Physics, int LogSize, FastPhysicsMatter Object, double MajorSpacing)
        {
            if (LogSize <= 0)
            {
                return Object;
            }
            if (LogSize > 1)
            {
                Object = _CreateLattice(Physics, LogSize - 1, Object, MajorSpacing * 0.5);
            }
            _Binary bina = _Combine(Physics, Object, Object, new Transform(new Vector(MajorSpacing, 0.0, 0.0)));
            _Binary binb = _Combine(Physics, bina, bina, new Transform(new Vector(0.0, MajorSpacing, 0.0)));
            _Binary binc = _Combine(Physics, binb, binb, new Transform(new Vector(0.0, 0.0, MajorSpacing)));
            return binc;
        }

        /// <summary>
        /// Combines two "bits" of matter into one without searching through cached compounds.
        /// </summary>
        private static FastPhysicsMatter._Binary _Combine(FastPhysics Physics, FastPhysicsMatter A, FastPhysicsMatter B, Transform AToB)
        {
            FastPhysicsMatter._Binary bin = new _Binary()
            {
                A = A,
                B = B,
                AToB = AToB
            };
            A._Usages.Add(bin);
            if (A != B)
            {
                B._Usages.Add(bin);
            }
            return bin;
        }

        /// <summary>
        /// Gets all the matter used to make up this matter, that is, all the matter needed to give this matter meaning, including
        /// this matter itself.
        /// </summary>
        internal virtual void _GetUsed(HashSet<FastPhysicsMatter> Elements)
        {
            Elements.Add(this);
        }

        /// <summary>
        /// Adds all particles in this matter to the given list after applying the specified transform.
        /// </summary>
        internal virtual void _GetParticles(Transform Transform, List<Vector> Particles)
        {
            
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

            internal override void _GetParticles(Transform Transform, List<Vector> Particles)
            {
                Particles.Add(Transform.Offset);
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

            internal override void _GetUsed(HashSet<FastPhysicsMatter> Elements)
            {
                Elements.Add(this);
                this.Source._GetUsed(Elements);
            }

            internal override void _GetParticles(Transform Transform, List<Vector> Particles)
            {
                this.Source._GetParticles(this.Transform.Apply(Transform), Particles);
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

            internal override void _GetUsed(HashSet<FastPhysicsMatter> Elements)
            {
                Elements.Add(this);
                this.A._GetUsed(Elements);
                this.B._GetUsed(Elements);
            }

            internal override void _GetParticles(Transform Transform, List<Vector> Particles)
            {
                this.A._GetParticles(Transform, Particles);
                this.B._GetParticles(this.AToB.Apply(Transform), Particles);
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

        public MatterDisparity GetDisparity(
            FastPhysics Physics, FastPhysicsMatter Environment, 
            double OldMass, double NewMass, 
            ISubstance NewSubstance, 
            Vector DeltaPosition, 
            Vector DeltaVelocity, 
            Quaternion DeltaOrientation, 
            AxisAngle OldSpin, AxisAngle NewSpin)
        {
            throw new NotImplementedException();
        }

        private UsageSet<FastPhysicsMatter._Particle> _Usages;
    }
}