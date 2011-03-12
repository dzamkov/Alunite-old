using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A mutable reference to an object.
    /// </summary>
    public class Mutable<T>
    {
        public Mutable(T Current)
        {
            this._Current = Current;
        }

        public static implicit operator T(Mutable<T> Ref)
        {
            return Ref.Current;
        }

        public static implicit operator Mutable<T>(T Current)
        {
            return new Mutable<T>(Current);
        }

        /// <summary>
        /// Gets the current referenced object.
        /// </summary>
        public T Current
        {
            get
            {
                return this._Current;
            }
        }

        /// <summary>
        /// Updates the item for this mutable reference.
        /// </summary>
        public void Update(T New)
        {
            this._Current = New;
            if (this.Changed != null)
            {
                this.Changed(New);
            }
        }

        /// <summary>
        /// An event fired when the current item is changed.
        /// </summary>
        public event ChangedHandler<T> Changed;

        private T _Current;
    }

    /// <summary>
    /// An event handler for a change in a mutable object.
    /// </summary>
    public delegate void ChangedHandler<T>(T New);
}