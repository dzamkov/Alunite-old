using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains the material properties for a particle excluding position, orientation, velocity and mass, which
    /// are common to all particles.
    /// </summary>
    public interface ISubstance
    {
        /// <summary>
        /// Updates a particle of this substance by the specified amount of time in seconds in the given environment (which is orientated to the particle). Assume
        /// that the units used in the Environment are consistent with those used for particles (meters, seconds, kilograms). This function should account for every
        /// force at every scale, including gravity and electromagnetism.
        /// </summary>
        void Update(Matter Environment, double Time, out Vector DPosition, out Vector DVelocity, out Quaternion DOrientation, ref double Mass);
    }

    /// <summary>
    /// Describes a particle in a certain frame of reference.
    /// </summary>
    public struct Particle
    {
        public Particle(Vector Position, Vector Velocity, Quaternion Orientation, double Mass, ISubstance Substance)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.Orientation = Orientation;
            this.Mass = Mass;
            this.Substance = Substance;
        }

        /// <summary>
        /// The gravitational constant which satisfies F = G * (M1 + M2) / (R * R) with F being the force of gravity in newtons, M1 and M2 the mass of the objects
        /// in kilograms and R the distance between them in meters.
        /// </summary>
        public static double G = 6.67428e-11;

        /// <summary>
        /// The relative location of the particle in meters.
        /// </summary>
        public Vector Position;

        /// <summary>
        /// The relative velocity of the particle in meters per second.
        /// </summary>
        public Vector Velocity;

        /// <summary>
        /// The relative orientation of the particle. This can be ignored for particles
        /// whose orientation doesn't matter.
        /// </summary>
        public Quaternion Orientation;

        /// <summary>
        /// The mass of the particle in kilograms.
        /// </summary>
        public double Mass;

        /// <summary>
        /// Gets the substance of the particle, which describes the particles properties.
        /// </summary>
        public ISubstance Substance;
    }
}