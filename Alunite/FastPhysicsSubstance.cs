using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
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
            Particle.Velocity.Z -= Time;
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