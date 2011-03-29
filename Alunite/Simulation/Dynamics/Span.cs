using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// The progression of an entity over time. A span based on physical laws must account for all interaction within an entity
    /// and possibly some external interaction with an environment (given by a span).
    /// </summary>
    public abstract class Span : Data<Span>
    {
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
        /// Gets the signal for the specified terminal from this span. If at any point the terminal is not in the span,
        /// or the terminal is inactive, the result of the signal will be "Nothing". All signals returned from this call will
        /// be unbounded in time, since the span itself is unbounded.
        /// </summary>
        public abstract Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal);

        /// <summary>
        /// Applies a uniform transform to this span. This will not affect the internal attributes of the span, nor the
        /// the output control signals.
        /// </summary>
        public virtual Span Apply(Transform Transform)
        {
            return new TransformedSpan(this, Transform);
        }

        /// <summary>
        /// Applies a path transform to this span. This will not affect the internal attributes of the span, nor the
        /// the output control signals.
        /// </summary>
        public virtual Span Apply(Signal<Transform> Path)
        {
            return new PathSpan(this, Path);
        }

        /// <summary>
        /// Creates a new span with initial parameters like this except for the environment and control input.
        /// </summary>
        public virtual Span Update(Span Environment, ControlInput Input)
        {
            return this.Initial.CreateSpan(Environment, Input);
        }

        /// <summary>
        /// Creates a superimposed combination two spans.
        /// </summary>
        public static Span Combine(Span Primary, Span Secondary)
        {
            if (Primary == Null)
            {
                return Secondary;
            }
            if (Secondary == Null)
            {
                return Primary;
            }
            return new BinarySpan(Primary, Secondary);
        }

        /// <summary>
        /// Creates a span for an entity with no external influence.
        /// </summary>
        public static Span Create(Entity Initial)
        {
            return Initial.CreateSpan(Span.Null, ControlInput.Null);
        }

        /// <summary>
        /// Gets the null span, which is a span containing the null entity at all times.
        /// </summary>
        public static NullSpan Null
        {
            get
            {
                return NullSpan.Singleton;
            }
        }
    }

    /// <summary>
    /// An span that has the state of a null entity at any time.
    /// </summary>
    public sealed class NullSpan : Span
    {
        private NullSpan()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly NullSpan Singleton = new NullSpan();

        public override Entity this[double Time]
        {
            get
            {
                return Entity.Null;
            }
        }

        public override Span Update(Span Environment, ControlInput Input)
        {
            return this;
        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            return Signal.Nothing<T>();
        }
    }

    /// <summary>
    /// A span that uniformly transforms all states of a source span. The transformation will only affect the external state
    /// of the span as entities can never know their absolute position, orientation or velocity.
    /// </summary>
    public class TransformedSpan : Span
    {
        public TransformedSpan(Span Source, Transform Transform)
        {
            this._Source = Source;
            this._Transform = Transform;
        }

        /// <summary>
        /// Gets the source before transformation.
        /// </summary>
        public Span Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the transformation.
        /// </summary>
        public Transform Transform
        {
            get
            {
                return this._Transform;
            }
        }

        public override Entity this[double Time]
        {
            get
            {
                return this._Source[Time].Apply(this._Transform);
            }
        }

        public override Span Update(Span Environment, ControlInput Input)
        {
            return this._Source.Update(Environment.Apply(this._Transform.Inverse), Input).Apply(this._Transform);
        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            return this._Source.Read(Terminal);
        }

        public override Span Apply(Transform Transform)
        {
            return new TransformedSpan(this._Source, Transform);
        }

        private Span _Source;
        private Transform _Transform;
    }

    /// <summary>
    /// Describes a mapping of signals to input terminals.
    /// </summary>
    public abstract class ControlInput
    {
        /// <summary>
        /// Gets the signal for the given input terminal.
        /// </summary>
        public abstract Signal<Maybe<T>> Lookup<T>(InTerminal<T> Terminal);

        /// <summary>
        /// Gets the null control input.
        /// </summary>
        public static NullControlInput Null
        {
            get
            {
                return NullControlInput.Singleton;
            }
        }
    }

    /// <summary>
    /// A control input that has the default ("Nothing") signal for all terminals.
    /// </summary>
    public class NullControlInput : ControlInput
    {
        private NullControlInput()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly NullControlInput Singleton = new NullControlInput();

        public override Signal<Maybe<T>> Lookup<T>(InTerminal<T> Terminal)
        {
            return Signal.Nothing<T>();
        }
    }
}