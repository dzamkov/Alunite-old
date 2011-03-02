using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Represents the similarity between objects. Similarity is described on a subjective exponential scale
    /// with a double. 0.0 indicates objects are identical while +infinity indicates objects are opposites. The geometric mean of 
    /// the similarity of any combination of objects of a common base should be near 1.0.
    /// </summary>
    public struct Similarity
    {
        public Similarity(double Value)
        {
            this.Value = Value;
        }

        /// <summary>
        /// Multiplies the weight of this similarity when used for comparison.
        /// </summary>
        public Similarity Weigh(double Amount)
        {
            return new Similarity(this.Value * Amount);
        }

        /// <summary>
        /// A similarity that indicates objects are identical.
        /// </summary>
        public static readonly Similarity Identical = new Similarity(0.0);

        /// <summary>
        /// Combines two similarity quantities.
        /// </summary>
        public static Similarity operator +(Similarity A, Similarity B)
        {
            return new Similarity(A.Value + B.Value);
        }

        /// <summary>
        /// Gets if A represents a stronger similarity than B.
        /// </summary>
        public static bool operator >(Similarity A, Similarity B)
        {
            return A.Value < B.Value;
        }

        /// <summary>
        /// Gets if B represents a weaker similarity than B.
        /// </summary>
        public static bool operator <(Similarity A, Similarity B)
        {
            return A.Value > B.Value;
        }
        
        /// <summary>
        /// Gets a similarity from a value.
        /// </summary>
        public static implicit operator Similarity(double Value)
        {
            return new Similarity(Value);
        }

        /// <summary>
        /// The double value of this similarity.
        /// </summary>
        public double Value;
    }
}