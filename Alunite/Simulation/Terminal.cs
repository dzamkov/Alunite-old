using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A reference to a bidirectional channel in an entity that can be used for communication with complimentary terminals over time.
    /// </summary>
    /// <remarks>Terminals are only compared by reference and therfore, need no additional information.</remarks>
    /// <typeparam name="TInput">The type of input received on the channel at any one time.</typeparam>
    /// <typeparam name="TOutput">The type of output given by the channel at any one time.</typeparam>
    public class Terminal<TInput, TOutput>
    {
        public override string ToString()
        {
            return this.GetHashCode().ToString();
        }
    }

    /// <summary>
    /// A mapping of terminals in one entity to those in another, usually larger and more complex entity.
    /// </summary>
    public abstract class TerminalMap
    {
        /// <summary>
        /// Gets the identity terminal map.
        /// </summary>
        public static IdentityTerminalMap Identity
        {
            get
            {
                return IdentityTerminalMap.Singleton;
            }
        }

        /// <summary>
        /// Gets a new lazy terminal map.
        /// </summary>
        public static LazyTerminalMap Lazy
        {
            get
            {
                return new LazyTerminalMap();
            }
        }

        /// <summary>
        /// Finds the corresponding terminal for the given local terminal.
        /// </summary>
        public abstract Terminal<TInput, TOutput> Lookup<TInput, TOutput>(Terminal<TInput, TOutput> Terminal);
    }

    /// <summary>
    /// A terminal map where lookup operations simply return the same terminal that is given.
    /// </summary>
    public class IdentityTerminalMap : TerminalMap
    {
        internal IdentityTerminalMap()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly IdentityTerminalMap Singleton = new IdentityTerminalMap();

        public override Terminal<TInput, TOutput> Lookup<TInput, TOutput>(Terminal<TInput, TOutput> Terminal)
        {
            return Terminal;
        }
    }

    /// <summary>
    /// A terminal map that only produces new terminals when they are looked up.
    /// </summary>
    public class LazyTerminalMap : TerminalMap
    {
        public LazyTerminalMap()
        {
            this._Terminals = new Dictionary<object, object>();
        }

        public override Terminal<TInput, TOutput> Lookup<TInput, TOutput>(Terminal<TInput, TOutput> Terminal)
        {
            object res;
            if (this._Terminals.TryGetValue(Terminal, out res))
            {
                return res as Terminal<TInput, TOutput>;
            }
            else
            {
                Terminal<TInput, TOutput> tres = new Terminal<TInput,TOutput>();
                this._Terminals[Terminal] = tres;
                return tres;
            }
        }

        private Dictionary<object, object> _Terminals;
    }
}