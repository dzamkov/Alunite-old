using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// The progression of an entity over time. A span must account for all interaction within an entity
    /// and possibly some external interaction with an environment (given by an entity).
    /// </summary>
    public abstract class Span
    {
        /// <summary>
        /// Gets the length of time this span occupies in seconds.
        /// </summary>
        public abstract double Length { get; }

        /// <summary>
        /// Gets the state of this span (as an entity) at the given time relative to the span.
        /// </summary>
        public abstract Entity this[double Time] { get; }

        /// <summary>
        /// Gets the initial state of the span.
        /// </summary>
        public virtual Entity Initial 
        {
            get
            {
                return this[0.0];
            }
        }
        
        /// <summary>
        /// Gets the initial environment (all entities that have an influence on this span, relative to the span) for this span.
        /// </summary>
        public abstract Entity InitialEnvironment { get; }

        /// <summary>
        /// Gets the signal for the specified terminal from this span. If at any point the terminal is not in the span,
        /// or the terminal is inactive, the result of the signal will be "Nothing".
        /// </summary>
        public abstract Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal);

        /// <summary>
        /// Creates a span for an entity with no external influence.
        /// </summary>
        public static Span Create(Entity Entity)
        {
            throw new NotImplementedException();
        }
    }
}