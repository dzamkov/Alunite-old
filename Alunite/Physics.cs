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
        TMatter Compose(IEnumerable<TMatter> Matter);

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

    /// <summary>
    /// Represents a possible the orientation, translation and velocity offset for matter.
    /// </summary>
    public struct Transform
    {
        public Transform(Vector Offset, Vector VelocityOffset, Quaternion Rotation)
        {
            this.Offset = Offset;
            this.VelocityOffset = VelocityOffset;
            this.Rotation = Rotation;
        }

        /// <summary>
        /// Applies this transform to another, in effect combining them.
        /// </summary>
        public Transform ApplyTo(Transform Transform)
        {
            return new Transform(
                this.Offset + this.Rotation.Rotate(Transform.Offset),
                this.VelocityOffset + this.Rotation.Rotate(Transform.VelocityOffset),
                this.Rotation.ApplyTo(Transform.Rotation));
        }

        /// <summary>
        /// Applies this transform to an offset vector.
        /// </summary>
        public Vector ApplyToOffset(Vector Offset)
        {
            return this.Offset + this.Rotation.Rotate(Offset);
        }

        /// <summary>
        /// Applies a transform to this transform.
        /// </summary>
        public Transform Apply(Transform Transform)
        {
            return Transform.ApplyTo(this);
        }

        /// <summary>
        /// Gets the inverse of this transform.
        /// </summary>
        public Transform Inverse
        {
            get
            {
                Quaternion nrot = this.Rotation.Conjugate;
                return new Transform(
                    nrot.Rotate(-this.Offset),
                    nrot.Rotate(-this.VelocityOffset),
                    nrot);
            }
        }

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        public static Transform Identity
        {
            get
            {
                return new Transform(
                    new Vector(0.0, 0.0, 0.0), 
                    new Vector(0.0, 0.0, 0.0), 
                    Quaternion.Identity);
            }
        }


        public Vector Offset;
        public Vector VelocityOffset;
        public Quaternion Rotation;
    }
}