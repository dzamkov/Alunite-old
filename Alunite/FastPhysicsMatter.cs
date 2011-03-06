using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
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
        public IEnumerable<Particle<FastPhysicsSubstance>> Particles
        {
            get
            {
                List<Particle<FastPhysicsSubstance>> parts = new List<Particle<FastPhysicsSubstance>>();
                this._GetParticles(Transform.Identity, parts);
                return parts;
            }
        }

        /// <summary>
        /// Applies a transform to this matter.
        /// </summary>
        public virtual FastPhysicsMatter Apply(FastPhysics Physics, Transform Transform)
        {
            return new _Transformed(this, Transform);
        }

        /// <summary>
        /// Gets the updated form of this matter in the specified environment after the given time.
        /// </summary>
        public abstract FastPhysicsMatter Update(FastPhysics Physics, FastPhysicsMatter Environment, double Time);

        /// <summary>
        /// Creates matter for a particle.
        /// </summary>
        public static FastPhysicsMatter Particle(FastPhysics Physics, Particle<FastPhysicsSubstance> Particle)
        {
            return new _Transformed(_Particle.Default, new Transform(Particle.Position, Particle.Velocity, Particle.Orientation));
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
        /// Combines two pieces of fast physics matter.
        /// </summary>
        public static FastPhysicsMatter Combine(FastPhysics Physics, FastPhysicsMatter A, FastPhysicsMatter B)
        {
            if (A == null)
            {
                return B;
            }
            if (B == null)
            {
                return A;
            }

            _Transformed atrans = A as _Transformed;
            _Transformed btrans = B as _Transformed;

            Transform atob = Transform.Identity;
            if (btrans != null)
            {
                atob = btrans.Transform;
                B = btrans.Source;
            }

            if (atrans != null)
            {
                return new _Binary(atrans.Source, B, atob.Apply(atrans.Transform.Inverse)).Apply(Physics, atrans.Transform);
            }
            else
            {
                return new _Binary(A, B, atob);
            }
        }

        /// <summary>
        /// Gets the mass, center of mass, and extent (distance from the center of mass to the farthest piece of matter) for this matter.
        /// </summary>
        public abstract void GetMass(out double Mass, out Vector CenterOfMass, out double Extent);

        /// <summary>
        /// Gets the force of gravity a particle at the specified offset and mass will feel from this matter.
        /// </summary>
        /// <param name="RecurseThreshold">The ratio of mass / (distance ^ 2) a piece of matter will have to have in order to have its
        /// gravity force "refined". Set at 0.0 to get the exact gravity.</param>
        public virtual Vector GetGravity(FastPhysics Physics, Vector Position, double Mass, double RecurseThreshold)
        {
            return Physics.GetGravity(Physics.GetMass(this), Position, Mass);
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
        internal virtual void _GetParticles(Transform Transform, List<Particle<FastPhysicsSubstance>> Particles)
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
            public static readonly _Particle Default = new _Particle() { Substance = FastPhysicsSubstance.Default, Mass = 1.0 };

            internal override void _GetParticles(Transform Transform, List<Particle<FastPhysicsSubstance>> Particles)
            {
                Particles.Add(new Particle<FastPhysicsSubstance>()
                {
                    Mass = this.Mass,
                    Spin = this.Spin,
                    Substance = this.Substance,
                    Orientation = Transform.Rotation,
                    Position = Transform.Offset,
                    Velocity = Transform.VelocityOffset
                });
            }

            public override void GetMass(out double Mass, out Vector CenterOfMass, out double Extent)
            {
                Mass = this.Mass;
                CenterOfMass = new Vector(0.0, 0.0, 0.0);
                Extent = 0.0;
            }

            public override FastPhysicsMatter Update(FastPhysics Physics, FastPhysicsMatter Environment, double Time)
            {
                Particle<FastPhysicsSubstance> part = new Particle<FastPhysicsSubstance>()
                {
                    Mass = this.Mass,
                    Spin = this.Spin,
                    Substance = this.Substance,
                    Orientation = Quaternion.Identity,
                    Position = new Vector(0.0, 0.0, 0.0),
                    Velocity = new Vector(0.0, 0.0, 0.0)
                };
                this.Substance.Update(Physics, Environment, Time, ref part);
                return new _Transformed(new _Particle()
                {
                    Mass = part.Mass,
                    Spin = part.Spin,
                    Substance = part.Substance
                }, new Transform(part.Position, part.Velocity, part.Orientation));
            }

            public override Vector GetGravity(FastPhysics Physics, Vector Position, double Mass, double RecurseThreshold)
            {
                return Physics.GetGravity(this.Mass, Position, Mass);
            }

            public FastPhysicsSubstance Substance;
            public double Mass;
            public AxisAngle Spin;
        }

        /// <summary>
        /// Matter created by transforming some source matter.
        /// </summary>
        internal class _Transformed : FastPhysicsMatter
        {
            public _Transformed(FastPhysicsMatter Source, Transform Transform)
            {
                this.Source = Source;
                this.Transform = Transform;
            }

            public override FastPhysicsMatter Apply(FastPhysics Physics, Transform Transform)
            {
                return new _Transformed(this.Source, this.Transform.Apply(Transform));
            }

            internal override void _GetUsed(HashSet<FastPhysicsMatter> Elements)
            {
                Elements.Add(this);
                this.Source._GetUsed(Elements);
            }

            internal override void _GetParticles(Transform Transform, List<Particle<FastPhysicsSubstance>> Particles)
            {
                this.Source._GetParticles(this.Transform.Apply(Transform), Particles);
            }

            public override void GetMass(out double Mass, out Vector CenterOfMass, out double Extent)
            {
                this.Source.GetMass(out Mass, out CenterOfMass, out Extent);
                CenterOfMass = this.Transform.ApplyToOffset(CenterOfMass);
            }

            public override Vector GetGravity(FastPhysics Physics, Vector Position, double Mass, double RecurseThreshold)
            {
                return this.Transform.ApplyToDirection(this.Source.GetGravity(Physics, this.Transform.Inverse.ApplyToDirection(Position), Mass, RecurseThreshold));
            }

            public override FastPhysicsMatter Update(FastPhysics Physics, FastPhysicsMatter Environment, double Time)
            {
                return Physics.Transform(this.Source.Update(Physics, Physics.Transform(Environment, this.Transform.Inverse), Time), this.Transform.Update(Time));
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
                : this(A, B, AToB, true)
            {

            }

            public _Binary(FastPhysicsMatter A, FastPhysicsMatter B, Transform AToB, bool Reuse)
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

                if (Reuse)
                {
                    A._Usages.Add(this);
                    if (A != B)
                    {
                        B._Usages.Add(this);
                    }
                }
            }

            public override FastPhysicsMatter Update(FastPhysics Physics, FastPhysicsMatter Environment, double Time)
            {
                FastPhysicsMatter na = this.A.Update(Physics, Combine(Physics, Environment, this.B.Apply(Physics, this.AToB)), Time);
                FastPhysicsMatter nb = this.B.Update(Physics, Combine(Physics, Environment, this.A).Apply(Physics, this.AToB.Inverse), Time);
                return Combine(Physics, na, nb.Apply(Physics, this.AToB));
            }

            internal override void _GetUsed(HashSet<FastPhysicsMatter> Elements)
            {
                Elements.Add(this);
                this.A._GetUsed(Elements);
                this.B._GetUsed(Elements);
            }

            internal override void _GetParticles(Transform Transform, List<Particle<FastPhysicsSubstance>> Particles)
            {
                this.A._GetParticles(Transform, Particles);
                this.B._GetParticles(this.AToB.Apply(Transform), Particles);
            }

            public override Vector GetGravity(FastPhysics Physics, Vector Position, double Mass, double RecurseThreshold)
            {
                Vector offset = Position - this.CenterOfMass;
                double offsetlen = offset.Length;

                double rat = this.Mass / (offsetlen * offsetlen);
                if (rat >= RecurseThreshold)
                {
                    return
                        A.GetGravity(Physics, Position, Mass, RecurseThreshold) +
                        this.AToB.ApplyToDirection(B.GetGravity(Physics, this.AToB.Inverse.ApplyToDirection(Position), Mass, RecurseThreshold));
                }
                else
                {
                    return offset * (Physics.GetGravityStrength(this.Mass, Mass, offsetlen) / offsetlen);
                }
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
}