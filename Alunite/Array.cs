using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A possibly-infinite collection of items stored at indices. This is a generalization of
    /// .net arrays and is closer to a function but arrays can be mutable. Unless otherwise stated,
    /// arrays may not change from outside influence when used in an argument.
    /// </summary>
    /// <typeparam name="S">The type of items in the array.</typeparam>
    /// <typeparam name="I">The type of indexes(or references) to the array.</typeparam>
    public interface IArray<T, I>
        where I : IEquatable<I>
    {
        /// <summary>
        /// Gets the item at the specified index. This function may throw an exception for some values, but it may
        /// not cause a modification in the array or the programs state (besides the side effects of the lookup).
        /// </summary>
        T Lookup(I Index);
    }

    /// <summary>
    /// An array whose elements may be changed.
    /// </summary>
    public interface IMutableArray<T, I>
        where I : IEquatable<I>
    {
        /// <summary>
        /// Modifies an element in the array to reflect a value. This function may throw an exception to indicate
        /// failure.
        /// </summary>
        void Modify(I Index, T Value);
    }

    /// <summary>
    /// An array where an in-place mapping can be applied.
    /// </summary>
    public interface IMapableArray<T, I> : IArray<T, I>
        where I : IEquatable<I>
    {
        /// <summary>
        /// Applies an in-place mapping, transforming all the elements in the array by the specified function.
        /// </summary>
        void Map(Func<T, T> Mapping);
    }

    /// <summary>
    /// An array whose intresting values can be enumerated.
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
    }

    /// <summary>
    /// A numerically-indexed array containing a finite amount of data. The "intresting values" (as defined by IFiniteArray)
    /// must be produced (using Values and Items) in sequential order starting at index 0 and stopping at the index before Count.
    /// </summary>
    public interface ISequentialArray<T> : IFiniteArray<T, int>
    {
        /// <summary>
        /// Gets a positive integer that defines the integer range [0, Count) where Values and Items iterates.
        /// </summary>
        int Count { get; }
    }

    /// <summary>
    /// A sequential array where items can be added at the end.
    /// </summary>
    public interface IExtendableArray<T> : ISequentialArray<T>
    {
        /// <summary>
        /// Adds an item to the array, incrementing its size.
        /// </summary>
        void Add(T Item);

        /// <summary>
        /// Adds an ordered collection of items to the array.
        /// </summary>
        void Add(IEnumerable<T> Items);
    }

    /// <summary>
    /// An array (by alunite's definition) created from a .net array.
    /// </summary>
    public class StandardArray<T> : IMutableArray<T, int>, ISequentialArray<T>, IMapableArray<T, int>
    {
        public StandardArray(T[] Items)
        {
            this._Items = Items;
        }

        public StandardArray(ISequentialArray<T> Source)
        {
            this._Items = new T[Source.Count];
            int i = 0;
            foreach (T item in Source.Values)
            {
                this._Items[i] = item;
                i++;
            }
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

        public StandardArray(IEnumerable<T> Source)
        {
            List<T> items = new List<T>(Source);
            this._Items = items.ToArray();
        }

        public void Map(Func<T, T> Mapping)
        {
            for (int t = 0; t < this._Items.Length; t++)
            {
                this._Items[t] = Mapping(this._Items[t]);
            }
        }

        /// <summary>
        /// Expands each item in the array in a 1:Amount ratio with another type.
        /// </summary>
        public StandardArray<F> Expand<F>(int Amount, Func<T, IEnumerable<F>> Mapping)
        {
            F[] otheritems = new F[this._Items.Length * Amount];
            int cur = 0;
            for (int t = 0; t < this._Items.Length; t++)
            {
                foreach (F f in Mapping(this._Items[t]))
                {
                    otheritems[cur] = f;
                    cur++;
                }
            }
            return new StandardArray<F>(otheritems);
        }

        public T Lookup(int Index)
        {
            if (Index >= 0 && Index < this._Items.Length)
            {
                return this._Items[Index];
            }
            else
            {
                return default(T);
            }
        }

        public void Modify(int Index, T Value)
        {
            this._Items[Index] = Value;
        }

        public IEnumerable<KeyValuePair<int, T>> Items
        {
            get
            {
                for (int t = 0; t < this._Items.Length; t++)
                {
                    yield return new KeyValuePair<int, T>(t, this._Items[t]);
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

        public int Count
        {
            get 
            {
                return this._Items.Length;
            }
        }

        public T Default
        {
            get 
            {
                return default(T);
            }
        }

        private T[] _Items;
    }

    /// <summary>
    /// A numerically-indexed array where items can be added and removed quickly.
    /// </summary>
    public class ListArray<T> : IMutableArray<T, int>, IExtendableArray<T>, IMapableArray<T, int>
    {
        public ListArray()
        {
            this._Items = new List<T>();
        }

        public ListArray(ISequentialArray<T> Source)
        {
            this._Items = new List<T>(Source.Values);
        }

        public ListArray(List<T> Source)
        {
            this._Items = Source;
        }

        public ListArray(IEnumerable<T> Source)
        {
            this._Items = new List<T>(Source);
        }

        public IEnumerable<T> Values
        {
            get 
            {
                return this._Items;
            }
        }

        public int Count
        {
            get 
            {
                return this._Items.Count;
            }
        }

        public T Default
        {
            get 
            {
                return default(T);
            }
        }

        public T Lookup(int Index)
        {
            if (Index >= 0 && Index < this._Items.Count)
            {
                return this._Items[Index];
            }
            else
            {
                return default(T);
            }
        }

        public IEnumerable<KeyValuePair<int, T>> Items
        {
            get
            {
                int i = 0;
                foreach (T item in this._Items)
                {
                    yield return new KeyValuePair<int, T>(i, item);
                    i++;
                }
            }
        }

        public void Modify(int Index, T Value)
        {
            this._Items[Index] = Value;
        }

        public void Add(T Item)
        {
            this._Items.Add(Item);
        }

        public void Add(IEnumerable<T> Items)
        {
            this._Items.AddRange(Items);
        }

        public void Map(Func<T, T> Mapping)
        {
            for (int t = 0; t < this._Items.Count; t++)
            {
                this._Items[t] = Mapping(this._Items[t]);
            }
        }

        private List<T> _Items;
    }
}