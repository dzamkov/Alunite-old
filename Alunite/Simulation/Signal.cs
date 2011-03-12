using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A continous time-varying value.
    /// </summary>
    /// <remarks>All ranges in signals represent the interval [Start, End).</remarks>
    /// <typeparam name="T">The type of the signal at any one time.</typeparam>
    public abstract class Signal<T> : IMutable<Void>
    {
        /// <summary>
        /// Gets the value of the signal at any one time. Not that no times before 0.0 may be used.
        /// </summary>
        /// <remarks>Due to the fact that a double can not perfectly represent any one time, this actually gets the average
        /// of the signal between Time and the next highest double.</remarks>
        public abstract T this[double Time] { get; }

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
    /// A maybe signal whose value is always nothing.
    /// </summary>
    public class NothingSignal<T> : Signal<Maybe<T>>
    {
        private NothingSignal()
        {

        }

        /// <summary>
        /// Gets the only instance of this signal.
        /// </summary>
        public static readonly NothingSignal<T> Singleton = new NothingSignal<T>();

        public override Maybe<T> this[double Time]
        {
            get
            {
                return Maybe<T>.Nothing;
            }
        }
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