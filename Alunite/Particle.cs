using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Descibes the physical properties of a particle.
    /// </summary>
    public interface ISubstance<TPhysics, TMatter>
        where TPhysics : IParticlePhysics<TPhysics, TMatter>
        where TMatter : IMatter
    {
        /// <summary>
        /// Updates a particle of this substance.
        /// </summary>
        void Update(TPhysics Physics, TMatter Environment, double Time, ref Vector Position, ref Vector Velocity, ref Quaternion Orientation, ref double Mass);
    }

    /// <summary>
    /// A physical system that allows the introduction and simulation of particles.
    /// </summary>
    public interface IParticlePhysics<TSelf, TMatter> : ISpatialPhysics<TMatter>
        where TSelf : IParticlePhysics<TSelf, TMatter>
        where TMatter : IMatter
    {
        /// <summary>
        /// Creates a matter form of a single particle with the specified properties.
        /// </summary>
        TMatter Create(ISubstance<TSelf, TMatter> Substance, Vector Position, Vector Velocity, Quaternion Orientation, double Mass);
    }
}