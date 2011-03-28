using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
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

        public Transform(Vector Offset)
        {
            this.Offset = Offset;
            this.VelocityOffset = new Vector(0.0, 0.0, 0.0);
            this.Rotation = Quaternion.Identity;
        }

        public Transform(double X, double Y, double Z)
            : this(new Vector(X, Y, Z))
        {

        }

        public Transform(StaticTransform Static, Vector VelocityOffset)
        {
            this.Offset = Static.Offset;
            this.Rotation = Static.Rotation;
            this.VelocityOffset = VelocityOffset;
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
        /// Applies this transform to a direction vector.
        /// </summary>
        public Vector ApplyToDirection(Vector Dir)
        {
            return this.Rotation.Rotate(Dir);
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
        /// Gets the static part of this transform.
        /// </summary>
        public StaticTransform Static
        {
            get
            {
                return new StaticTransform(this.Offset, this.Rotation);
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
                    Vector.Zero,
                    Vector.Zero,
                    Quaternion.Identity);
            }
        }

        /// <summary>
        /// Updates the state of this transform after the given amount of time in seconds.
        /// </summary>
        public Transform Update(double Time)
        {
            return new Transform(this.Offset + this.VelocityOffset * Time, this.VelocityOffset, this.Rotation);
        }

        /// <summary>
        /// Gets the matrix representation of the transformation applied to offsets.
        /// </summary>
        public AfflineMatrix OffsetMatrix
        {
            get
            {
                return new AfflineMatrix(this.Rotation, this.Offset);
            }
        }

        public Vector Offset;
        public Vector VelocityOffset;
        public Quaternion Rotation;
    }

    /// <summary>
    /// A transform for matter not accounting for a change velocity.
    /// </summary>
    public struct StaticTransform
    {
        public StaticTransform(Vector Offset, Quaternion Rotation)
        {
            this.Offset = Offset;
            this.Rotation = Rotation;
        }

        public StaticTransform(Vector Offset)
        {
            this.Offset = Offset;
            this.Rotation = Quaternion.Identity;
        }

        public StaticTransform(double X, double Y, double Z)
            : this(new Vector(X, Y, Z))
        {

        }

        /// <summary>
        /// Gets the inverse of this transform.
        /// </summary>
        public StaticTransform Inverse
        {
            get
            {
                Quaternion nrot = this.Rotation.Conjugate;
                return new StaticTransform(nrot.Rotate(-this.Offset), nrot);
            }
        }

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        public static StaticTransform Identity
        {
            get
            {
                return new StaticTransform(
                    Vector.Zero,
                    Quaternion.Identity);
            }
        }

        /// <summary>
        /// Applies this transform to another, in effect combining them.
        /// </summary>
        public StaticTransform ApplyTo(StaticTransform Transform)
        {
            return new StaticTransform(this.Offset + this.Rotation.Rotate(Transform.Offset), this.Rotation.ApplyTo(Transform.Rotation));
        }

        /// <summary>
        /// Applies this transform to an offset vector.
        /// </summary>
        public Vector ApplyToOffset(Vector Offset)
        {
            return this.Offset + this.Rotation.Rotate(Offset);
        }

        /// <summary>
        /// Applies this transform to a direction vector.
        /// </summary>
        public Vector ApplyToDirection(Vector Dir)
        {
            return this.Rotation.Rotate(Dir);
        }

        /// <summary>
        /// Applies a transform to this transform.
        /// </summary>
        public StaticTransform Apply(StaticTransform Transform)
        {
            return Transform.ApplyTo(this);
        }

        public static implicit operator Transform(StaticTransform A)
        {
            return new Transform(A, Vector.Zero);
        }

        /// <summary>
        /// Gets the matrix representation of the transformation.
        /// </summary>
        public AfflineMatrix Matrix
        {
            get
            {
                return new AfflineMatrix(this.Rotation, this.Offset);
            }
        }

        public Vector Offset;
        public Quaternion Rotation;
    }
}