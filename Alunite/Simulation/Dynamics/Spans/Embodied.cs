using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A span representing a control span embodied within a physical span.
    /// </summary>
    public class EmbodiedSpan : Span
    {
        public EmbodiedSpan(Span Control, Span Body)
        {
            this._Control = Control;
            this._Body = Body;
        }

        public override Entity this[double Time]
        {
            get
            {
                return this._Control[Time].Embody(this._Body[Time]);
            }
        }

        public override Entity Initial
        {
            get
            {
                return this._Control.Initial.Embody(this._Body.Initial);
            }
        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            return Signal.Defer(this._Control.Read(Terminal), this._Body.Read(Terminal));
        }

        /// <summary>
        /// Creates an embodied span given the initial control entity, the spans for the body and the environment (for the system) and
        /// the control input to the system.
        /// </summary>
        public static Span Create(Entity InitialControl, Span Body, Span Environment, ControlInput Input)
        {
            Signal<Maybe<Transform>> path = GetPath(Body);
            return
                new EmbodiedSpan(
                    InitialControl.CreateSpan(Span.Combine(Body, Environment).Apply(Signal.Default(path, Transform.Identity)), Input),
                    Body);
        }

        /// <summary>
        /// Gets the path of a physical body through space, or nothing if the body is decimated to the point where this becomes a 
        /// meaningless question.
        /// </summary>
        public static Signal<Maybe<Transform>> GetPath(Span Body)
        {
            return Signal.Just(Signal.Constant(Transform.Identity));
        }

        public override Span Update(Span Environment, ControlInput Input)
        {
            Span nbody = this._Body.Update(Environment, Input);
            Signal<Maybe<Transform>> path = GetPath(nbody);
            Span ncontrol = this._Control.Update(Span.Combine(nbody, Environment).Apply(Signal.Default(path, Transform.Identity)), Input);
            return new EmbodiedSpan(ncontrol, nbody);
        }

        private Span _Control;
        private Span _Body;
    }
}