using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An object that can store all the values of its given type plus a Nothing value.
    /// </summary>
    public struct Maybe<T>
    {
        public Maybe(T Value)
        {
            this.Data = Value;
            this.IsNothing = false;
        }

        /// <summary>
        /// Gets the Nothing value.
        /// </summary>
        public static Maybe<T> Nothing
        {
            get
            {
                return new Maybe<T>()
                {
                    Data = default(T),
                    IsNothing = true
                };
            }
        }

        /// <summary>
        /// Gets a data maybe value.
        /// </summary>
        public static Maybe<T> Just(T Data)
        {
            return new Maybe<T>(Data);
        }

        /// <summary>
        /// Tries to get the data from this maybe value. Returns true if it exists or false if this
        /// is a Nothing value.
        /// </summary>
        public bool TryGetData(out T Data)
        {
            if (this.IsNothing)
            {
                Data = default(T);
                return false;
            }
            else
            {
                Data = this.Data;
                return true;
            }
        }

        /// <summary>
        /// The data for the structure if Nothing is set to false.
        /// </summary>
        public T Data;

        /// <summary>
        /// Is this value Nothing?
        /// </summary>
        public bool IsNothing;
    }
}