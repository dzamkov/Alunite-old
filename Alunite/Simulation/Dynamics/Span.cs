using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A natural (governed by physical laws) progression of an entity given as a signal.
    /// </summary>
    public abstract class Span : Signal<Entity>
    {
        /// <summary>
        /// Gets the initial entity this span is for.
        /// </summary>
        public virtual Entity Initial
        {
            get
            {
                return this[0.0];
            }
        }

        /// <summary>
        /// Gets the signal corresponding to an output terminal. If the terminal is not in the span, or is not active, nothing
        /// will be returned.
        /// </summary>
        public abstract Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal);

        /// <summary>
        /// Creates a span representing the natural progression of an entity in a given environment with the given a terminal input. The resulting span
        /// will have a length equal to that of the environment. All terminal input signals should be at or exceed this length.
        /// </summary>
        public static Span Create(Entity Initial, Signal<Entity> Environment, TerminalInput Input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an unbound span representing the natural progression of an entity with no outside environment.
        /// </summary>
        public static Span Create(Entity Initial)
        {
            return Create(Initial, Null, TerminalInput.Null);
        }

        /// <summary>
        /// Creates a natural span with the same initial entity as this one but with a different environment or terminal input.
        /// </summary>
        public virtual Span Update(Signal<Entity> Environment, TerminalInput Input)
        {
            return Create(this.Initial, Environment, Input);
        }

        /// <summary>
        /// Gets the null span.
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
    /// A span with an indefinite length.
    /// </summary>
    public abstract class UnboundSpan : Span
    {
        public override double Length
        {
            get
            {
                return double.PositiveInfinity;
            }
        }
    }

    /// <summary>
    /// The span for a null entity.
    /// </summary>
    public class NullSpan : UnboundSpan
    {
        private NullSpan()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly NullSpan Singleton = new NullSpan();

        public override Entity Initial
        {
            get
            {
                return Entity.Null;
            }
        }

        public override Entity this[double Time]
        {
            get
            {
                return Entity.Null;
            }
        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            return Signal.Nothing<T>();
        }
    }

    /// <summary>
    /// Gives signals for input terminals for a span. Note that this object is not guranteed to be immutable, and
    /// should not be stored when it is passed as a parameter.
    /// </summary>
    public abstract class TerminalInput
    {
        /// <summary>
        /// Gets the signal given to the specified terminal. All signals returned by this method are unbound.
        /// </summary>
        public abstract Signal<Maybe<T>> Read<T>(InTerminal<T> Terminal);

        /// <summary>
        /// Gets the null "TerminalInput".
        /// </summary>
        public static NullTerminalInput Null
        {
            get
            {
                return NullTerminalInput.Null;
            }
        }
    }

    /// <summary>
    /// Terminal input with no active terminals.
    /// </summary>
    public class NullTerminalInput : TerminalInput
    {
        private NullTerminalInput()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly NullTerminalInput Singleton = new NullTerminalInput();

        public override Signal<Maybe<T>> Read<T>(InTerminal<T> Terminal)
        {
            return Signal.Nothing<T>();
        }
    }
}