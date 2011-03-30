using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// The unbounded progression of an entity over time possibly through natural interactions with other spans.
    /// </summary>
    public abstract class Span : Data<Span>
    {
        /// <summary>
        /// Gets an entity that represents the state of this span at the given time relative to the begining of the span.
        /// </summary>
        public abstract Entity this[double Time] { get; }

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
        /// Creates a span representing the natural progression of an entity in a given environment with the given a terminal input.
        /// </summary>
        public static Span Natural(Entity Initial, Span Environment, TerminalInput Input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a span representing the natural progression of an entity with no outside environment.
        /// </summary>
        public static Span Natural(Entity Initial)
        {
            throw new NotImplementedException();
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
    /// A span which gives a null entity at any moment and contains no active terminals.
    /// </summary>
    public class NullSpan : Span
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