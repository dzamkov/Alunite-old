using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Represents an infinitely precise, continous time-varying value.
    /// </summary>
    /// <remarks>All ranges in signals represent the interval [Start, End).</remarks>
    /// <typeparam name="T">The type of the signal at any one time or a range of time.</typeparam>
    public abstract class Signal<T> : Data<Signal<T>>
    {
        /// <summary>
        /// Gets the value of the signal at the specified time.
        /// </summary>
        public abstract T this[double Time] { get; }
    }

    /// <summary>
    /// A maybe signal whose value is always nothing.
    /// </summary>
    public sealed class NothingSignal<T> : Signal<Maybe<T>>
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
    /// Encapsulates a source signal into a maybe signal with no "Nothing" values.
    /// </summary>
    public sealed class JustSignal<T> : Signal<Maybe<T>>
    {
        public JustSignal(Signal<T> Source)
        {
            this._Source = Source;
        }

        /// <summary>
        /// Gets the source signal for this signal.
        /// </summary>
        public Signal<T> Source
        {
            get
            {
                return this._Source;
            }
        }

        public override Maybe<T> this[double Time]
        {
            get
            {
                return Maybe<T>.Just(this._Source[Time]);
            }
        }

        public override Signal<Maybe<T>> Simplify
        {
            get
            {
                this._Source = this._Source.Simplify;
                return this;
            }
        }

        private Signal<T> _Source;
    }

    /// <summary>
    /// A signal that takes two maybe sources signals and at any time takes the value of the primary unless it is 
    /// nothing, in which case it takes the value of the secondary.
    /// </summary>
    public sealed class DeferSignal<T> : Signal<Maybe<T>>
    {
        public DeferSignal(Signal<Maybe<T>> Primary, Signal<Maybe<T>> Secondary)
        {
            this._Primary = Primary;
            this._Secondary = Secondary;
        }

        /// <summary>
        /// Gets the primary signal.
        /// </summary>
        public Signal<Maybe<T>> Primary
        {
            get
            {
                return this._Primary;
            }
        }

        /// <summary>
        /// Gets the secondary signal.
        /// </summary>
        public Signal<Maybe<T>> Secondary
        {
            get
            {
                return this._Secondary;
            }
        }

        public override Maybe<T> this[double Time]
        {
            get
            {
                Maybe<T> p = this._Primary[Time];
                if (p.IsNothing)
                {
                    return this._Secondary[Time];
                }
                else
                {
                    return p;
                }
            }
        }

        public override Signal<Maybe<T>> Simplify
        {
            get
            {
                this._Primary = this._Primary.Simplify;
                this._Secondary = this._Secondary.Simplify;
                if (this._Primary == NothingSignal<T>.Singleton)
                {
                    return this._Secondary;
                }
                if (this._Primary is JustSignal<T>)
                {
                    return this._Primary;
                }

                JustSignal<T> js = this._Secondary as JustSignal<T>;
                if (js != null)
                {
                    return new JustSignal<T>(new DefaultSignal<T>(this._Primary, js.Source));
                }
                return this;
            }
        }

        private Signal<Maybe<T>> _Primary;
        private Signal<Maybe<T>> _Secondary;
    }

    /// <summary>
    /// A signal which returns values from Primary that are not nothing, or values from Secondary when they are.
    /// </summary>
    public sealed class DefaultSignal<T> : Signal<T>
    {
        public DefaultSignal(Signal<Maybe<T>> Primary, Signal<T> Secondary)
        {
            this._Primary = Primary;
            this._Secondary = Secondary;
        }

        /// <summary>
        /// Gets the primary signal.
        /// </summary>
        public Signal<Maybe<T>> Primary
        {
            get
            {
                return this._Primary;
            }
        }

        /// <summary>
        /// Gets the secondary signal.
        /// </summary>
        public Signal<T> Secondary
        {
            get
            {
                return this._Secondary;
            }
        }

        public override T this[double Time]
        {
            get
            {
                Maybe<T> p = this._Primary[Time];
                T val;
                if (p.TryGetData(out val))
                {
                    return val;
                }
                else
                {
                    return this._Secondary[Time];
                }
            }
        }

        public override Signal<T> Simplify
        {
            get
            {
                this._Primary = this._Primary.Simplify;
                this._Secondary = this._Secondary.Simplify;

                if (this._Primary == NothingSignal<T>.Singleton)
                {
                    return this._Secondary;
                }

                JustSignal<T> js = this._Primary as JustSignal<T>;
                if (js != null)
                {
                    return js.Source;
                }
                return this;
            }
        }

        private Signal<Maybe<T>> _Primary;
        private Signal<T> _Secondary;
    }

    /// <summary>
    /// A signal with a constant value.
    /// </summary>
    public sealed class ConstantSignal<T> : Signal<T>
    {
        public ConstantSignal(T Value)
        {
            this._Value = Value;
        }

        /// <summary>
        /// Gets the value of this constant signal at any time.
        /// </summary>
        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        public override T this[double Time]
        {
            get
            {
                return this._Value;
            }
        }

        public override Signal<T> Simplify
        {
            get
            {
                // TODO: Make nothing signals for nothing values
                return this;
            }
        }

        private T _Value;
    }

    /// <summary>
    /// A signal containing a finite amount of discrete events.
    /// </summary>
    /// <remarks>Since events occupy an infinitesimal amount of time, reading from this signal the standard way (using a time parameter) will never return
    /// any events.</remarks>
    public class EventSignal<T> : Signal<Multi<T>>
    {
        public EventSignal(IEnumerable<Event<T>> Events)
        {
            this._Events = new List<Event<T>>(Events);
        }

        public EventSignal(List<Event<T>> Events)
        {
            this._Events = Events;
        }

        public override Multi<T> this[double Time]
        {
            get
            {
                return Multi<T>.Null;
            }
        }

        /// <summary>
        /// Gets the events in the signal.
        /// </summary>
        public IEnumerable<Event<T>> Events
        {
            get
            {
                return this._Events;
            }
        }

        /// <summary>
        /// Gets the value of the closest event before (or equal to) the given time, or the default value if there
        /// are no events before that time.
        /// </summary>
        public T Previous(T Default, double Time)
        {
            List<Event<T>> e = this._Events;
            int l = 0;
            int h = e.Count;

            if (h == 0 || Time < e[0].Time)
            {
                return Default;
            }

            while (true)
            {
                int s = (l + h) / 2;
                Event<T> st = e[s];
                if (Time > st.Time)
                {
                    l = s;
                    if (l + 1 == h)
                    {
                        return st.Item;
                    }
                }
                else
                {
                    h = s;
                    if (l + 1 == h)
                    {
                        return e[l].Item;
                    }
                }
            }
        }

        private List<Event<T>> _Events;
    }

    /// <summary>
    /// Signal-related functions.
    /// </summary>
    public static class Signal
    {
        /// <summary>
        /// Gets a maybe signal whose value is always nothing.
        /// </summary>
        public static NothingSignal<T> Nothing<T>()
        {
            return NothingSignal<T>.Singleton;
        }

        /// <summary>
        /// Encapsulates a signal as a maybe signal with no nothing values.
        /// </summary>
        public static Signal<Maybe<T>> Just<T>(Signal<T> Signal)
        {
            return new JustSignal<T>(Signal);
        }

        /// <summary>
        /// Creates a signal that returns the value from the primary signal if its not nothing, or gets the value from the secondary signal otherwise.
        /// </summary>
        public static Signal<T> Default<T>(Signal<Maybe<T>> Primary, Signal<T> Secondary)
        {
            return new DefaultSignal<T>(Primary, Secondary);
        }

        /// <summary>
        /// Creates a signal that replaces all nothings in the primary signal with the given value.
        /// </summary>
        public static Signal<T> Default<T>(Signal<Maybe<T>> Primary, T Secondary)
        {
            return Default<T>(Primary, Constant<T>(Secondary));
        }

        /// <summary>
        /// Creates a defered signal with the specified primary and secondary signals.
        /// </summary>
        public static Signal<Maybe<T>> Defer<T>(Signal<Maybe<T>> Primary, Signal<Maybe<T>> Secondary)
        {
            return new DeferSignal<T>(Primary, Secondary);
        }

        /// <summary>
        /// Gets a signal with a constant value.
        /// </summary>
        public static ConstantSignal<T> Constant<T>(T Value)
        {
            return new ConstantSignal<T>(Value);
        }

        /// <summary>
        /// Creates a signal with the specified discrete events.
        /// </summary>
        public static EventSignal<T> Discrete<T>(IEnumerable<Event<T>> Events)
        {
            return new EventSignal<T>(Events);
        }

        /// <summary>
        /// Gets the discrete events from a signal, or returns null if there is an infinite amount of events.
        /// </summary>
        public static IEnumerable<Event<T>> Events<T>(Signal<Multi<T>> Signal)
        {
            EventSignal<T> es = Signal as EventSignal<T>;
            if (es != null)
            {
                return es.Events;
            }

            ConstantSignal<Multi<T>> cs = Signal as ConstantSignal<Multi<T>>;
            if (cs != null)
            {
                if (cs.Value.Empty)
                {
                    return new Event<T>[0];
                }
                else
                {
                    return null;
                }
            }

            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A sample type that contains a variable amount of items (as events) of the given type.
    /// </summary>
    /// <remarks>This can be used as the type parameter of a signal to create an event signal.</remarks>
    public struct Multi<T>
    {
        public Multi(IEnumerable<T> Items)
        {
            this.Items = Items;
        }

        /// <summary>
        /// Gets a sample with no items.
        /// </summary>
        public static Multi<T> Null
        {
            get
            {
                return new Multi<T>(null);
            }
        }

        /// <summary>
        /// Gets if the multi sample contains no items.
        /// </summary>
        public bool Empty
        {
            get
            {
                return this.Items == null || !this.Items.GetEnumerator().MoveNext();
            }
        }

        /// <summary>
        /// The chronologically-ordered collection of all items in the period of the sample.
        /// </summary>
        public IEnumerable<T> Items;
    }

    /// <summary>
    /// A chronologically-placed item.
    /// </summary>
    public struct Event<T>
    {
        public Event(T Item, double Time)
        {
            this.Item = Item;
            this.Time = Time;
        }

        /// <summary>
        /// The data for the event.
        /// </summary>
        public T Item;

        /// <summary>
        /// The time at which the event occurs.
        /// </summary>
        public double Time;
    }
}