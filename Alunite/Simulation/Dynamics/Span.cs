using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A progression of an entity given as a signal.
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
        public static Span Natural(Entity Initial, Signal<Entity> Environment, TerminalInput Input)
        {
            return new NaturalSpan(Initial, Environment, Input);
        }

        /// <summary>
        /// Creates an unbound span representing the natural progression of an entity with no outside environment.
        /// </summary>
        public static Span Natural(Entity Initial)
        {
            return Natural(Initial, Null, TerminalInput.Null);
        }

        /// <summary>
        /// Creates a natural span with the same initial entity as this one but with a different environment or terminal input.
        /// </summary>
        public virtual Span UpdateNatural(Signal<Entity> Environment, TerminalInput Input)
        {
            return Natural(this.Initial, Environment, Input);
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
    /// Represents the natural (governed by physical laws) progression of an entity with a given
    /// environment and terminal input.
    /// </summary>
    public class NaturalSpan : Span
    {
        public NaturalSpan(Entity Initial, Signal<Entity> Environment, TerminalInput Input)
        {
            this._Initial = Initial;
            this._Input = Input;
            this._Environment = Environment;
        }

        /// <summary>
        /// Gets the terminal input for the natural span.
        /// </summary>
        public TerminalInput Input
        {
            get
            {
                return this._Input;
            }
        }

        /// <summary>
        /// Gets the environment for the entity.
        /// </summary>
        public Signal<Entity> Environment
        {
            get
            {
                return this._Environment;
            }
        }

        public override Entity Initial
        {
            get
            {
                return this._Initial;    
            }
        }

        public override double Length
        {
            get
            {
                return this._Environment.Length;
            }
        }

        public override Entity this[double Time]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            throw new NotImplementedException();
        }

        private Entity _Initial;
        private Signal<Entity> _Environment;
        private TerminalInput _Input;
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
    /// Gives signals for input terminals for a span.
    /// </summary>
    public abstract class TerminalInput
    {
        /// <summary>
        /// Gets the signal given to the specified terminal.
        /// </summary>
        public abstract Signal<Maybe<T>> Read<T>(InTerminal<T> Terminal);

        /// <summary>
        /// Gets the null "TerminalInput".
        /// </summary>
        public static NullTerminalInput Null
        {
            get
            {
                return NullTerminalInput.Singleton;
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