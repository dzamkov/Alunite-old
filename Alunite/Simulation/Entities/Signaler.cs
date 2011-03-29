using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A simple entity which outputs an unbounded immutable signal of a certain type to a terminal.
    /// </summary>
    public class SignalerEntity<T> : PhantomEntity
    {
        public SignalerEntity(Signal<T> Signal)
        {
            this._Signal = Signal;
            this._Terminal = new OutTerminal<T>();
        }

        public SignalerEntity(Signal<T> Signal, OutTerminal<T> Terminal)
        {
            this._Signal = Signal;
            this._Terminal = Terminal;
        }

        /// <summary>
        /// Gets the signal that is "played" on the terminal.
        /// </summary>
        public Signal<T> Signal
        {
            get
            {
                return this._Signal;
            }
        }

        /// <summary>
        /// Gets the terminal that the signal is outputted to.
        /// </summary>
        public OutTerminal<T> Terminal
        {
            get
            {
                return this._Terminal;
            }
        }

        private Signal<T> _Signal;
        private OutTerminal<T> _Terminal;
    }
}