﻿using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains methods for manipulation of continous values of a comman base. 
    /// </summary>
    /// <remarks>"Continuum" is a fancy word for number line.</remarks>
    public interface IContinuum<T> : IMultiplication<T, double, T>
    {
        /// <summary>
        /// Adds two values.
        /// </summary>
        T Add(T A, T B);

        /// <summary>
        /// Subtracts one value from another.
        /// </summary>
        T Subtract(T A, T B);

        /// <summary>
        /// Gets a value which has no effect when added or subtracted from another value.
        /// </summary>
        T Zero { get; }

        /// <summary>
        /// Gets a value between the two given values. If amount is 0.0, the returned value will be A and if amount is 1.0, the
        /// returned value will be B, with the value being interpolated between the two.
        /// </summary>
        T Mix(T A, T B, double Amount);
    }

    /// <summary>
    /// Describes a multiplication between objects. The multiplication may not be commutative.
    /// </summary>
    public interface IMultiplication<TA, TB, TResult>
    {
        /// <summary>
        /// Gets the product of two values.
        /// </summary>
        TResult Multiply(TA A, TB B);
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

        public double Zero
        {
            get
            {
                return 0.0;
            }
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

        public Vector Zero
        {
            get
            {
                return Vector.Zero;
            }
        }
    }
}