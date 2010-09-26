using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A finite enumerable collection of items stored at indices. This is a generalization of
    /// .net arrays.
    /// </summary>
    /// <typeparam name="S">The type of items in the array.</typeparam>
    /// <typeparam name="I">The type of indexes(or references) to the array.</typeparam>
    public interface IArray<T, I>
        where I : IEquatable<I>
    {
        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        T Item(I Index);

        /// <summary>
        /// Gets the items in the array (in no particular order).
        /// </summary>
        IEnumerable<KeyValuePair<I, T>> Items { get; }

        /// <summary>
        /// Gets the values in the array (in no particular order).
        /// </summary>
        IEnumerable<T> Values { get; }
    }

    /// <summary>
    /// An array (by alunite's definition) created from a .net array.
    /// </summary>
    public class StandardArray<T> : IArray<T, int>
    {
        public StandardArray(T[] Items)
        {
            this._Items = Items;
        }

        public T Item(int Index)
        {
            return this._Items[Index];
        }

        public IEnumerable<KeyValuePair<int, T>> Items
        {
            get 
            {
                for (int i = 0; i < this._Items.Length; i++)
                {
                    yield return new KeyValuePair<int, T>(i, this._Items[i]);
                }
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                return this._Items;
            }
        }

        private T[] _Items;
    }
}