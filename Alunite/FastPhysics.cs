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
            throw new NotImplementedException();
        }

        public FastPhysicsMatter Transform(FastPhysicsMatter Matter, Transform Transform)
        {
            throw new NotImplementedException();
        }

        public FastPhysicsMatter Update(FastPhysicsMatter Matter, FastPhysicsMatter Environment, double Time)
        {
            throw new NotImplementedException();
        }

        public FastPhysicsMatter Compose(IEnumerable<FastPhysicsMatter> Matter)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Matter in a fast physics system.
    /// </summary>
    public class FastPhysicsMatter : IMatter
    {

    }

    /// <summary>
    /// A substance in a fast physics system.
    /// </summary>
    public class FastPhysicsSubstance : IAutoSubstance<FastPhysics, FastPhysicsMatter, FastPhysicsSubstance>
    {
        private FastPhysicsSubstance()
        {

        }

        /// <summary>
        /// The default (and currently only) possible substance.
        /// </summary>
        public static readonly FastPhysicsSubstance Default = new FastPhysicsSubstance();

        public void Update(FastPhysics Physics, FastPhysicsMatter Environment, double Time, ref Particle<FastPhysicsSubstance> Particle)
        {
            Particle.Update(Time);
        }
    }
}