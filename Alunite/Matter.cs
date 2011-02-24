using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Represents an untransformed object that can participate in physical interactions.
    /// </summary>
    public abstract class Matter
    {
        /// <summary>
        /// Creates the updated form of this matter given the environment (which is all matter in the world excluding, and given 
        /// in the frame of reference of the matter in question) by a given amount of time in seconds.
        /// </summary>
        public abstract Element Update(Matter Environment, double Time);
    }

    /// <summary>
    /// Matter made by a physical composition of other matter.
    /// </summary>
    public abstract class CompositeMatter : Matter
    {
        /// <summary>
        /// Gets the pieces of matter that makes up this matter. The order should not matter (lol).
        /// </summary>
        public abstract IEnumerable<Element> Elements { get; }
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
        public Transform Apply(Transform Transform)
        {
            return new Transform(
                this.Offset + this.Rotation.Rotate(Transform.Offset),
                this.VelocityOffset + this.Rotation.Rotate(Transform.VelocityOffset),
                this.Rotation * Transform.Rotation);
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

        public Vector Offset;
        public Vector VelocityOffset;
        public Quaternion Rotation;
    }

    /// <summary>
    /// A piece of transformed matter.
    /// </summary>
    public struct Element
    {
        /// <summary>
        /// The original matter for this element.
        /// </summary>
        public Matter Source;
    }
}