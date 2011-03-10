using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Represents an object that can have additive and subtractive operations applied to it. Note that it can be assumed that the default
    /// Operand is the additive identity.
    /// </summary>
    /// <typeparam name="TBase">The base of the objects produced by this kind of arithmetic.</typeparam>
    /// <typeparam name="TOperand">The allowable operand for the arithmetic.</typeparam>
    public interface IAdditive<TBase, TOperand>
    {
        /// <summary>
        /// Gets the sum of this and a operand.
        /// </summary>
        TBase Add(TOperand Operand);

        /// <summary>
        /// Gets the difference of this and a operand.
        /// </summary>
        TBase Subtract(TOperand Operand);
    }

    /// <summary>
    /// Represents an object that can have multiplicative and divise operations applied to it.
    /// </summary>
    /// <typeparam name="TBase">The base of the objects produced by this kind of arithmetic.</typeparam>
    /// <typeparam name="TOperand">The allowable operand for the arithmetic.</typeparam>
    public interface IMultiplicative<TBase, TOperand>
    {
        /// <summary>
        /// Gets the product of this and a operand.
        /// </summary>
        TBase Multiply(TOperand Operand);

        /// <summary>
        /// Gets the quotient of this and a operand.
        /// </summary>
        TBase Divide(TOperand Operand);
    }

    /// <summary>
    /// Represents an object that has a square root.
    /// </summary>
    public interface ISquareRootable<TBase> : IMultiplicative<TBase, TBase>
    {
        /// <summary>
        /// Gets the square root of this object.
        /// </summary>
        TBase SquareRoot { get; }
    }
}