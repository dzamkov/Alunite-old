using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Represents the progression of an entity over time. Simulations can be interacted with using
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
        /// Gets a signal for the output of the given terminal. Note that the signal will update as the simulation updates (if any corrections are made
        /// to input signals).
        /// </summary>
        public abstract Signal<TOutput> Read<TInput, TOutput>(Terminal<TInput, TOutput> Terminal);

        /// <summary>
        /// Sets the input of the given terminal to the specified signal.
        /// </summary>
        public abstract void Write<TInput, TOutput>(Terminal<TInput, TOutput> Terminal, Signal<TInput> Signal);
    }

    /// <summary>
    /// A concrete implementation of simulation.
    /// </summary>
    internal class _Simulation : Simulation
    {
        public _Simulation(Entity World)
        {

        }

        public override Signal<TOutput> Read<TInput, TOutput>(Terminal<TInput, TOutput> Terminal)
        {
            throw new NotImplementedException();
        }

        public override void Write<TInput, TOutput>(Terminal<TInput, TOutput> Terminal, Signal<TInput> Signal)
        {
            throw new NotImplementedException();
        }
    }
}