using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A collection of items stored at indices. This is a generalization of
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
        T Lookup(I Index);
    }

    /// <summary>
    /// An array whose elements can be enumerated. Enumerable arrays must have a finite amount
    /// of items.
    /// </summary>
    public interface IFiniteArray<T, I> : IArray<T, I>
        where I : IEquatable<I>
    {
        /// <summary>
        /// Gets the items in the array (in no particular order).
        /// </summary>
        IEnumerable<KeyValuePair<I, T>> Items { get; }

        /// <summary>
        /// Gets the values in the array (in no particular order).
        /// </summary>
        IEnumerable<T> Values { get; }

        /// <summary>
        /// Gets the size of the array.
        /// </summary>
        int Size { get; }
    }

    /// <summary>
    /// An array (by alunite's definition) created from a .net array.
    /// </summary>
    public class StandardArray<T> : IFiniteArray<T, int>
    {
        public StandardArray(T[] Items)
        {
            this._Items = Items;
        }

        public StandardArray(IEnumerable<T> Items, int Count)
        {
            this._Items = new T[Count];
            IEnumerator<T> e = Items.GetEnumerator();
            for (int t = 0; t < this._Items.Length; t++)
            {
                if (e.MoveNext())
                {
                    this._Items[t] = e.Current;
                }
                else
                {
                    break;
                }
            }
        }

        public StandardArray(IFiniteArray<T, int> Source)
        {
            this._Items = new T[Source.Size];
            foreach (KeyValuePair<int, T> item in Source.Items)
            {
                this._Items[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// Creates an array by transforming every element in this array by a mapping function.
        /// </summary>
        public StandardArray<F> Map<F>(Func<T, F> Mapping)
        {
            F[] otheritems = new F[this._Items.Length];
            for (int t = 0; t < otheritems.Length; t++)
            {
                otheritems[t] = Mapping(this._Items[t]);
            }
            return new StandardArray<F>(otheritems);
        }

        public T Lookup(int Index)
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

        public int Size
        {
            get
            {
                return this._Items.Length;
            }
        }

        private T[] _Items;
    }
}