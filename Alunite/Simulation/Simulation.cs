﻿using System;
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
        /// Creates a connection to a terminal by specifing an input signal and reciving an output signal. Both
        /// signals may be updated along with the simulation. If either signal is Nothing at any time, it
        /// indicates that the terminal is inactive in the corresponding direction.
        /// </summary>
        public abstract Mutable<Signal<Maybe<TOutput>>> Connect<TInput, TOutput>(Mutable<Signal<Maybe<TInput>>> Input, Terminal<TInput, TOutput> Terminal);

        /// <summary>
        /// Creates a one-way connection with a terminal that only gives information.
        /// </summary>
        public void Write<TInput, TOutput>(Mutable<Signal<Maybe<TInput>>> Input, Terminal<TInput, TOutput> Terminal)
        {
            this.Connect<TInput, TOutput>(Input, Terminal);
        }

        /// <summary>
        /// Creates a one-way connection with a terminal that only receives information.
        /// </summary>
        public Mutable<Signal<Maybe<TOutput>>> Read<TInput, TOutput>(Terminal<TInput, TOutput> Terminal)
        {
            return this.Connect((Mutable<Signal<Maybe<TInput>>>)NothingSignal<TInput>.Singleton, Terminal);
        }
    }

    /// <summary>
    /// A concrete implementation of simulation.
    /// </summary>
    internal class _Simulation : Simulation
    {
        public _Simulation(Entity World)
        {

        }

        public override Mutable<Signal<Maybe<TOutput>>> Connect<TInput, TOutput>(Mutable<Signal<Maybe<TInput>>> Input, Terminal<TInput, TOutput> Terminal)
        {
            throw new NotImplementedException();
        }
    }
}