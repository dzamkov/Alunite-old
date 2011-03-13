using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A mutable representation of the progression of an entity over time. Simulations can be interacted with using
    /// unbound terminals in the supplied entity.
    /// </summary>
    public abstract class Simulation
    {
        /// <summary>
        /// Creates a simulation with the given initial state.
        /// </summary>
        public static Simulation Create(Entity World)
        {
            return new _Simulation(World);
        }

        /// <summary>
        /// Reads the signal from the given terminal. The signal will be Nothing when the terminal is not
        /// active, either because the entity that has the terminal does not have the required terminal or
        /// the entity does not exist.
        /// </summary>
        public abstract Mutable<Signal<Maybe<T>>> Read<T>(OutTerminal<T> Terminal);

        /// <summary>
        /// Writes a signal to the given terminal. Note that only one input source may be used
        /// for each input terminal.
        /// </summary>
        public abstract void Write<T>(InTerminal<T> Terminal, Mutable<Signal<Maybe<T>>> Signal);
    }

    /// <summary>
    /// A concrete implementation of simulation.
    /// </summary>
    internal class _Simulation : Simulation
    {
        public _Simulation(Entity World)
        {

        }

        public override Mutable<Signal<Maybe<T>>> Read<T>(OutTerminal<T> Terminal)
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(InTerminal<T> Terminal, Mutable<Signal<Maybe<T>>> Signal)
        {
            throw new NotImplementedException();
        }
    }
}