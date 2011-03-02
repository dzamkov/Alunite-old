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
                this.Rotation * Transform.Rotation);
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

        public Similarity GetSimilarity(Transform Other, double OffsetWeight, double VelocityWeight, double RotationWeight)
        {
            return
                this.Offset.GetOffsetSimilarity(Other.Offset).Weigh(OffsetWeight) +
                this.VelocityOffset.GetOffsetSimilarity(Other.VelocityOffset).Weigh(VelocityWeight) +
                this.Rotation.GetRotationSimilarity(Other.Rotation).Weigh(RotationWeight);
        }

        public Vector Offset;
        public Vector VelocityOffset;
        public Quaternion Rotation;
    }
}