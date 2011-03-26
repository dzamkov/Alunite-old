using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Describes a method of interpolating between two values of a common base.
    /// </summary>
    public interface IInterpolation<T>
    {
        /// <summary>
        /// Interpolates A and B with the given amount between 0.0 (A) and 1.0 (B).
        /// </summary>
        T Mix(T A, T B, double Amount);
    }

    /// <summary>
    /// Describes a linear interpolation between vectors.
    /// </summary>
    public class VectorInterpolation : IInterpolation<Vector>
    {
        private VectorInterpolation()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly VectorInterpolation Singleton = new VectorInterpolation();

        public Vector Mix(Vector A, Vector B, double Amount)
        {
            return A * (1.0 - Amount) + B * Amount;
        }
    }

    /// <summary>
    /// Describes a linear interpolation between scalars (doubles).
    /// </summary>
    public class ScalarInterpolation : IInterpolation<double>
    {
        private ScalarInterpolation()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly ScalarInterpolation Singleton = new ScalarInterpolation();

        public double Mix(double A, double B, double Amount)
        {
            return A * (1.0 - Amount) + B * Amount;
        }
    }
}