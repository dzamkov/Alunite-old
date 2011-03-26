using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains methods for manipulation of continous values of a comman base. 
    /// </summary>
    /// <remarks>"Continuum" is a fancy word for number line.</remarks>
    public interface IContinuum<T>
    {
        /// <summary>
        /// Multiplies a value by a scalar.
        /// </summary>
        T Multiply(T Value, double Scalar);

        /// <summary>
        /// Adds two values.
        /// </summary>
        T Add(T A, T B);

        /// <summary>
        /// Subtracts one value from another.
        /// </summary>
        T Subtract(T A, T B);

        /// <summary>
        /// Gets a value between the two given values. If amount is 0.0, the returned value will be A and if amount is 1.0, the
        /// returned value will be B, with the value being interpolated between the two.
        /// </summary>
        T Mix(T A, T B, double Amount);
    }

    /// <summary>
    /// A continuum for scalar (real) values.
    /// </summary>
    public struct ScalarContinuum : IContinuum<double>
    {
        public double Multiply(double Value, double Scalar)
        {
            return Value * Scalar;
        }

        public double Add(double A, double B)
        {
            return A + B;
        }

        public double Subtract(double A, double B)
        {
            return A - B;
        }

        public double Mix(double A, double B, double Amount)
        {
            return A * (1.0 - Amount) + B * Amount;
        }
    }

    /// <summary>
    /// A continuum for vector values.
    /// </summary>
    public struct VectorContinuum : IContinuum<Vector>
    {
        public Vector Multiply(Vector Value, double Scalar)
        {
            return Value * Scalar;
        }

        public Vector Add(Vector A, Vector B)
        {
            return A + B;
        }

        public Vector Subtract(Vector A, Vector B)
        {
            return A - B;
        }

        public Vector Mix(Vector A, Vector B, double Amount)
        {
            return A * (1.0 - Amount) + B * Amount;
        }
    }
}