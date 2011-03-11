using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A continous time-varying value.
    /// </summary>
    /// <typeparam name="T">The type of the signal at any one time.</typeparam>
    public abstract class Signal<T> : IMutable<Void>
    {
        /// <summary>
        /// Gets the value of the signal at any one time.
        /// </summary>
        /// <remarks>Due to the fact that a double can not perfectly represent any one time, this actually gets the average
        /// of the signal between Time and the next highest double.</remarks>
        public abstract T this[double Time] { get; }

        /// <summary>
        /// Gets the sum of the values in the range [Start, End) divided by the size of interval.
        /// </summary>
        public abstract T Average(double Start, double End);

        /// <summary>
        /// Informs the subscribers of this signal that it has been changed.
        /// </summary>
        protected void OnChange()
        {
            if (this.Changed != null)
            {
                this.Changed(Void.Value);
            }
        }

        public event ChangedHandler<Void> Changed;
    }

    /// <summary>
    /// A sample type that contains a variable amount of items of the given type.
    /// </summary>
    /// <remarks>This can be used as the type parameter of a signal to create an event signal.</remarks>
    public struct Event<T>
    {
        /// <summary>
        /// Gets if the event sample contains no events.
        /// </summary>
        public bool Empty
        {
            get
            {
                return !this.Items.GetEnumerator().MoveNext();
            }
        }

        /// <summary>
        /// The chronologically-ordered collection of all events in the period of the sample.
        /// </summary>
        public IEnumerable<T> Items;
    }
}