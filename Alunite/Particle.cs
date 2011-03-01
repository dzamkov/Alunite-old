using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// A physical system that allows the introduction and simulation of particles.
    /// </summary>
    public interface IParticlePhysics<TMatter, TSubstance> : ISpatialPhysics<TMatter>
        where TMatter : IMatter
        where TSubstance : ISubstance
    {
        /// <summary>
        /// Creates a matter form of a single particle with the specified properties.
        /// </summary>
        TMatter Create(Particle<TSubstance> Particle);
    }

    /// <summary>
    /// Contains the properties of a particle.
    /// </summary>
    public struct Particle<TSubstance>
    {
        /// <summary>
        /// The substance the particle is of.
        /// </summary>
        public TSubstance Substance;

        /// <summary>
        /// The relative position of the particle.
        /// </summary>
        public Vector Position;

        /// <summary>
        /// The relative velocity of the particle.
        /// </summary>
        public Vector Velocity;

        /// <summary>
        /// The orientation of the particle.
        /// </summary>
        public Quaternion Orientation;

        /// <summary>
        /// The angular velocity of the particle.
        /// </summary>
        public Quaternion Spin;

        /// <summary>
        /// The mass of the particle in kilograms.
        /// </summary>
        public double Mass;

        /// <summary>
        /// Updates the spatial state of this particle by the given amount of time in seconds.
        /// </summary>
        public void Update(double Time)
        {
            this.Position += this.Velocity * Time;
        }
    }

    /// <summary>
    /// Descibes the physical properties of a particle.
    /// </summary>
    public interface ISubstance
    {
        
    }

    /// <summary>
    /// A substance with known interactions in a certain kind of physics system.
    /// </summary>
    public interface IAutoSubstance<TPhysics, TMatter, TSubstance> : ISubstance
        where TPhysics : IParticlePhysics<TMatter, TSubstance>
        where TMatter : IMatter
        where TSubstance : IAutoSubstance<TPhysics, TMatter, TSubstance>
    {
        /// <summary>
        /// Updates a particle of this kind of substance in the given environment.
        /// </summary>
        void Update(TPhysics Physics, TMatter Environment, double Time, ref Particle<TSubstance> Particle);
    }
}