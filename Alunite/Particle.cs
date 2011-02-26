using System;
using System.Collections.Generic;
using System.Linq;

using OpenTKGUI;

namespace Alunite
{
    /// <summary>
    /// Contains the material properties for a particle excluding position, orientation, velocity and mass, which
    /// are common to all particles.
    /// </summary>
    public interface ISubstance
    {
        /// <summary>
        /// Updates a particle of this substance by the specified amount of time in seconds in the given environment. This function should account for every
        /// force at every scale, including gravity and electromagnetism.
        /// </summary>
        ISubstance Update(Matter Environment, double Time, ref Vector Position, ref Vector Velocity, ref Quaternion Orientation, ref double Mass);
    }

    /// <summary>
    /// A substance with some visual properties.
    /// </summary>
    public interface IVisualSubstance : ISubstance
    {
        /// <summary>
        /// Gets the color this substance should be displayed with.
        /// </summary>
        Color Color { get; }
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

        public Particle(Vector Position, double Mass, ISubstance Substance)
        {
            this.Position = Position;
            this.Velocity = new Vector(0.0, 0.0, 0.0);
            this.Orientation = Quaternion.Identity;
            this.Mass = Mass;
            this.Substance = Substance;
        }

        public Particle(Transform Transform, double Mass, ISubstance Substance)
        {
            this.Position = Transform.Offset;
            this.Velocity = Transform.VelocityOffset;
            this.Orientation = Transform.Rotation;
            this.Mass = Mass;
            this.Substance = Substance;
        }

        /// <summary>
        /// Gets the transform of the particle from the origin.
        /// </summary>
        public Transform Transform
        {
            get
            {
                return new Transform(
                    this.Position,
                    this.Velocity,
                    this.Orientation);
            }
        }

        /// <summary>
        /// Gets a matter representation of this particle.
        /// </summary>
        public Matter Matter
        {
            get
            {
                return new ParticleMatter(this.Substance, this.Mass).Apply(this.Transform);
            }
        }

        /// <summary>
        /// Gets the particle with a transform applied to it.
        /// </summary>
        public Particle Apply(Transform Transform)
        {
            return new Particle(Transform.ApplyTo(this.Transform), this.Mass, this.Substance);
        }

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

    /// <summary>
    /// Matter containing a single unoriented particle.
    /// </summary>
    public class ParticleMatter : Matter
    {
        public ParticleMatter(ISubstance Substance, double Mass)
        {
            this._Substance = Substance;
            this._Mass = Mass;
        }

        /// <summary>
        /// Gets the substance of the particle.
        /// </summary>
        public ISubstance Substance
        {
            get
            {
                return this._Substance;
            }
        }

        /// <summary>
        /// Gets the mass of the particle.
        /// </summary>
        public double Mass
        {
            get
            {
                return this._Mass;
            }
        }

        public override IEnumerable<Particle> Particles
        {
            get
            {
                return new Particle[1]
                {
                    new Particle(new Vector(0.0, 0.0, 0.0), new Vector(0.0, 0.0, 0.0), Quaternion.Identity, this._Mass, this._Substance)
                };
            }
        }


        public override Matter Update(Matter Environment, double Time)
        {
            Vector pos = new Vector(0.0, 0.0, 0.0);
            Vector vel = new Vector(0.0, 0.0, 0.0);
            Quaternion ort = Quaternion.Identity;
            double mass = this._Mass;
            ISubstance nsub = this._Substance.Update(Environment, Time, ref pos, ref vel, ref ort, ref mass);
            return
                new TransformMatter(
                    new ParticleMatter(nsub, mass),
                    new Transform(pos, vel, ort));
        }

        private ISubstance _Substance;
        private double _Mass;
    }
}