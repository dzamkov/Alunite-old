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
            _Binary bina = new _Binary(Object, Object, new Transform(new Vector(MajorSpacing, 0.0, 0.0)));
            _Binary binb = new _Binary(bina, bina, new Transform(new Vector(0.0, MajorSpacing, 0.0)));
            _Binary binc = new _Binary(binb, binb, new Transform(new Vector(0.0, 0.0, MajorSpacing)));
            return binc;
        }

        /// <summary>
        /// Gets the mass, center of mass, and extent (distance from the center of mass to the farthest piece of matter) for this matter.
        /// </summary>
        public abstract void GetMass(out double Mass, out Vector CenterOfMass, out double Extent);

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
            public static readonly _Particle Default = new _Particle() { Mass = 1.0 };

            internal override void _GetParticles(Transform Transform, List<Vector> Particles)
            {
                Particles.Add(Transform.Offset);
            }

            public override void GetMass(out double Mass, out Vector CenterOfMass, out double Extent)
            {
                Mass = this.Mass;
                CenterOfMass = new Vector(0.0, 0.0, 0.0);
                Extent = 0.0;
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

            internal override void _GetUsed(HashSet<FastPhysicsMatter> Elements)
            {
                Elements.Add(this);
                this.Source._GetUsed(Elements);
            }

            internal override void _GetParticles(Transform Transform, List<Vector> Particles)
            {
                this.Source._GetParticles(this.Transform.Apply(Transform), Particles);
            }

            public override void GetMass(out double Mass, out Vector CenterOfMass, out double Extent)
            {
                this.Source.GetMass(out Mass, out CenterOfMass, out Extent);
                CenterOfMass = this.Transform.ApplyToOffset(CenterOfMass);
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
            public _Binary(FastPhysicsMatter A, FastPhysicsMatter B, Transform AToB)
            {
                this.A = A;
                this.B = B;
                this.AToB = AToB;

                double amass; Vector acen; double aext; A.GetMass(out amass, out acen, out aext);
                double bmass; Vector bcen; double bext; B.GetMass(out bmass, out bcen, out bext); bcen = this.AToB.ApplyToOffset(bcen);
                this.Mass = amass + bmass;
                this.CenterOfMass = acen * (amass / this.Mass) + bcen * (bmass / this.Mass);

                double alen = (acen - this.CenterOfMass).Length + aext;
                double blen = (bcen - this.CenterOfMass).Length + bext;
                this.Extent = Math.Max(alen, blen);

                A._Usages.Add(this);
                if (A != B)
                {
                    B._Usages.Add(this);
                }
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

            public override void GetMass(out double Mass, out Vector CenterOfMass, out double Extent)
            {
                Mass = this.Mass;
                CenterOfMass = this.CenterOfMass;
                Extent = this.Extent;
            }

            public double Mass;
            public Vector CenterOfMass;
            public double Extent;
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