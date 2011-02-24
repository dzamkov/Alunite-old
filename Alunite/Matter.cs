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