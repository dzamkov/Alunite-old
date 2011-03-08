using System;
using System.Collections.Generic;

using OpenTK;

namespace Alunite
{
    /// <summary>
    /// Represents a point or offset in one-dimensional space.
    /// </summary>
    public struct Scalar : IAdditive<Scalar, Scalar>, IMultiplicative<Scalar, Scalar>
    {
        public Scalar(double Value)
        {
            this.Value = Value;
        }

        public static implicit operator double(Scalar Scalar)
        {
            return Scalar.Value;
        }

        public static implicit operator Scalar(double Value)
        {
            return new Scalar(Value);
        }

        public Scalar Add(Scalar Operand)
        {
            return this + Operand;
        }

        public Scalar Subtract(Scalar Operand)
        {
            return this - Operand;
        }

        public Scalar Multiply(Scalar Operand)
        {
            return this * Operand;
        }

        public Scalar Divide(Scalar Operand)
        {
            return this / Operand;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public double Value;
    }
}