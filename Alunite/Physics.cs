using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Represents a set of physical laws that allow the interaction of matter over time.
    /// </summary>
    public interface IPhysics<TMatter>
        where TMatter : IMatter
    {
        /// <summary>
        /// Gets the updated state of some matter after some time elapses. The environment is all the matter acting
        /// upon the target matter.
        /// </summary>
        TMatter Update(TMatter Matter, TMatter Environment, double Time);

        /// <summary>
        /// Creates some matter that is the physical composition of other matter.
        /// </summary>
        TMatter Compose(IEnumerable<TMatter> Elements);

        /// <summary>
        /// Gets matter that has no affect or interaction in the physics system.
        /// </summary>
        TMatter Null { get; }
    }

    /// <summary>
    /// A physical system that acts in three-dimensional space.
    /// </summary>
    public interface ISpatialPhysics<TMatter> : IPhysics<TMatter>
        where TMatter : IMatter
    {
        /// <summary>
        /// Applies a transformation to some matter.
        /// </summary>
        TMatter Transform(TMatter Matter, Transform Transform);
    }

    /// <summary>
    /// A physical system where all matter has a mass expressable in kilograms.
    /// </summary>
    public interface IMassPhysics<TMatter> : IPhysics<TMatter>
        where TMatter : IMatter
    {
        /// <summary>
        /// Gets the mass of the given matter.
        /// </summary>
        double GetMass(TMatter Matter);
    }

    /// <summary>
    /// A physical system where gravity (a force that affects all matter depending only on their mass) exists. Note that gravity must be
    /// a force felt mutally by involved matter.
    /// </summary>
    public interface IGravitationalPhysics<TMatter> : ISpatialPhysics<TMatter>, IMassPhysics<TMatter>
        where TMatter : IMatter
    {
        /// <summary>
        /// Gets the gravity (meters / second ^ 2) a particle at the specified position and mass will feel if it was in the given environment.
        /// </summary>
        Vector GetGravity(TMatter Environment, Vector Position, double Mass);
    }

    /// <summary>
    /// Represents an untransformed object that can participate in physical interactions.
    /// </summary>
    public interface IMatter
    {

    }

    /// <summary>
    /// Describes the difference between two pieces of matter.
    /// </summary>
    /// <remarks>The "simple matter disparity" does not take into account forces to external matter while the "complex matter disparity" does. Note that
    /// matter disparity is for the most part an approximation used for optimizations.</remarks>
    public struct MatterDisparity
    {
        public MatterDisparity(double Movement, double Translation, double Mass)
        {
            this.Movement = Movement;
            this.Translation = Translation;
            this.Mass = Mass;
        }

        /// <summary>
        /// Gets the matter disparity between two identical pieces of matter.
        /// </summary>
        public static MatterDisparity Identical
        {
            get
            {
                return new MatterDisparity(0.0, 0.0, 0.0);
            }
        }
        
        /// <summary>
        /// Gets the simple matter disparity between two particles.
        /// </summary>
        public static MatterDisparity BetweenParticles(double MassA, Vector PosA, Vector VelA, double MassB, Vector PosB, Vector VelB)
        {
            return new MatterDisparity(
                (MassA + MassB) * (VelA - VelB).Length, 
                (MassA + MassB) * (PosA - PosB).Length, 
                Math.Abs(MassA - MassB));
        }

        /// <summary>
        /// The amount of mass, course deviation that would be caused if the compared pieces of matter were swapped in usage, measured in
        /// kilograms meters per second. Note that if this is for "complex matter disparity", this should take into account external
        /// matter and forces.
        /// </summary>
        public double Movement;

        /// <summary>
        /// The amount of mass that is immediately translated between the two compared pieces of matter, measured in kilogram meters.
        /// </summary>
        public double Translation;

        /// <summary>
        /// The difference in mass of the two compared pieces of matter measured in kilograms.
        /// </summary>
        public double Mass;
    }
}