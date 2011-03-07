using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite.Fast
{
    /// <summary>
    /// Matter containing a single, untransformed particle.
    /// </summary>
    public class ParticleMatter : Matter
    {
        public ParticleMatter(Substance Substance, double Mass, AxisAngle Spin)
        {
            this._Substance = Substance;
            this._Mass = Mass;
            this._Spin = Spin;
        }

        /// <summary>
        /// Gets the substance of the particle represented by this matter.
        /// </summary>
        public Substance Substance
        {
            get
            {
                return this._Substance;
            }
        }

        /// <summary>
        /// Gets the untransformed spin of the particle represented by this matter.
        /// </summary>
        public AxisAngle Spin
        {
            get
            {
                return this._Spin;
            }
        }

        public override void OutputParticles(Transform Transform, List<Particle<Substance>> Particles)
        {
            Particles.Add(new Particle<Substance>()
            {
                Mass = this._Mass,
                Spin = this._Spin.Apply(Transform.Rotation),
                Substance = this._Substance,
                Orientation = Transform.Rotation,
                Position = Transform.Offset,
                Velocity = Transform.VelocityOffset
            });
        }

        public override void GetMassSummary(Physics Physics, out double Mass, out Vector CenterOfMass, out double Extent)
        {
            Mass = this._Mass;
            CenterOfMass = new Vector(0.0, 0.0, 0.0);
            Extent = 0.0;
        }

        public override Matter Update(Physics Physics, Matter Environment, double Time)
        {
            Particle<Substance> part = new Particle<Substance>()
            {
                Mass = this._Mass,
                Spin = this._Spin,
                Substance = this._Substance,
                Orientation = Quaternion.Identity,
                Position = new Vector(0.0, 0.0, 0.0),
                Velocity = new Vector(0.0, 0.0, 0.0)
            };
            this.Substance.Update(Physics, Environment, Time, ref part);
            return Physics.Create(part);
        }

        public override Vector GetGravity(Physics Physics, Vector Position, double Mass, double RecurseThreshold)
        {
            return Physics.GetGravity(this._Mass, Position, Mass);
        }

        private Substance _Substance;
        private double _Mass;
        private AxisAngle _Spin;
    }
}