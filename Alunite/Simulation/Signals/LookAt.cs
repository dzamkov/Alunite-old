using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A signal which, at any moment, gives a transform that aligns an entity at (0.0, 0.0, 0.0) looking towards (1.0, 0.0, 0.0) with
    /// (0.0, 0.0, 1.0) as the up vector to be positioned and look in a certain direction.
    /// </summary>
    public class LookAtSignal : Signal<Transform>
    {
        public LookAtSignal(Signal<Vector> Position, Signal<Vector> Velocity, Signal<Vector> Foward, Signal<Vector> Up)
        {
            this._Position = Position;
            this._Velocity = Velocity;
            this._Foward = Foward;
            this._Up = Up;
        }

        public LookAtSignal(Signal<Vector> Position, Signal<Vector> Foward, Signal<Vector> Up)
            : this(Position, Signal.Derivative(Position), Foward, Up)
        {

        }

        /// <summary>
        /// Gets a signal that gives the offset of the transform at any time.
        /// </summary>
        public Signal<Vector> Position
        {
            get
            {
                return this._Position;
            }
        }

        /// <summary>
        /// Gets a signal that gives the velocity offset of the transform at any time. By default, this is the derivative of position.
        /// </summary>
        public Signal<Vector> Velocity
        {
            get
            {
                return this._Velocity;
            }
        }

        /// <summary>
        /// Gets a vector which indicates the foward direction (or the direction that is looked towards) at any time.
        /// </summary>
        public Signal<Vector> Foward
        {
            get
            {
                return this._Foward;
            }
        }

        /// <summary>
        /// Gets a vector which indicates the up direction at any time.
        /// </summary>
        public Signal<Vector> Up
        {
            get
            {
                return this._Up;
            }
        }

        public override Signal<Transform> Simplify
        {
            get
            {
                this._Position = this._Position.Prefered;
                this._Velocity = this._Velocity.Prefered;
                this._Foward = this._Foward.Prefered;
                this._Up = this._Up.Prefered;
                return this;
            }
        }

        public override Transform this[double Time]
        {
            get
            {
                Vector pos = this._Position[Time];
                Vector vel = this._Velocity[Time];
                Vector fow = this._Foward[Time];
                Vector up = this._Up[Time];

                // This could probably be faster
                OrthogonalMatrix om = OrthogonalMatrix.Lookat(Vector.Normalize(fow), Vector.Normalize(up));
                Quaternion rot = Quaternion.FromMatrix(om);

                return new Transform(pos, vel, rot);
            }
        }

        public override double Length
        {
            get
            {
                double len = this._Position.Length;
                len = Math.Min(len, this._Foward.Length);
                len = Math.Min(len, this._Up.Length);
                len = Math.Min(len, this._Velocity.Length);
                return len;
            }
        }

        private Signal<Vector> _Position;
        private Signal<Vector> _Velocity;
        private Signal<Vector> _Foward;
        private Signal<Vector> _Up;
    }
}