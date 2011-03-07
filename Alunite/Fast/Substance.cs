using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite.Fast
{
    /// <summary>
    /// A substance in a fast physics system.
    /// </summary>
    public class Substance : IAutoSubstance<Physics, Matter, Substance>
    {
        private Substance()
        {

        }

        /// <summary>
        /// The default (and currently only) possible substance.
        /// </summary>
        public static readonly Substance Default = new Substance();

        public void Update(Physics Physics, Matter Environment, double Time, ref Particle<Substance> Particle)
        {
            Particle.Velocity += Environment.GetGravity(Physics, new Vector(0.0, 0.0, 0.0), Particle.Mass, Physics.G * 1.0e15) * (Time / Particle.Mass);
            Particle.Update(Time);
        }

        public MatterDisparity GetDisparity(
            Physics Physics, Matter Environment,
            double OldMass, double NewMass,
            ISubstance NewSubstance,
            Vector DeltaPosition,
            Vector DeltaVelocity,
            Quaternion DeltaOrientation,
            AxisAngle OldSpin, AxisAngle NewSpin)
        {
            throw new NotImplementedException();
        }
    }
}