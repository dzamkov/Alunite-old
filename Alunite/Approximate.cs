using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// An object that can be loosely compared to another object with the same base. Similarity is described on a subjective exponential scale
    /// with a double. 0.0 indicates objects are identical while +infinity indicates objects are opposites. The geometric mean of 
    /// the similarity of any combination of objects of a common base should be near 1.0.
    /// </summary>
    /// <remarks>The similarity between numbers is the absolute value of their difference.</remarks>
    public interface IApproximatable<TBase>
        where TBase : IApproximatable<TBase>
    {
        /// <summary>
        /// Gets the similarity this object has with another.
        /// </summary>
        double GetSimilarity(TBase Object);
    }
}