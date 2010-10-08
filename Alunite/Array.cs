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

    /// <summary>
    /// Acts as an array that has a one to one mapping to a source array.
    /// </summary>
    public class MapSequentialArray<T, F> : ISequentialArray<F>
    {
        public MapSequentialArray(ISequentialArray<T> Source, Func<T, F> Mapping)
        {
            this._Source = Source;
            this._Mapping = Mapping;
        }

        public int Count
        {
            get 
            {
                return this._Source.Count;
            }
        }

        public IEnumerable<KeyValuePair<int, F>> Items
        {
            get 
            {
                foreach (KeyValuePair<int, T> items in this._Source.Items)
                {
                    yield return new KeyValuePair<int, F>(items.Key, this._Mapping(items.Value));
                }
            }
        }

        public IEnumerable<F> Values
        {
            get 
            {
                foreach (T val in this._Source.Values)
                {
                    yield return this._Mapping(val);
                }
            }
        }

        public F Lookup(int Index)
        {
            return this._Mapping(this._Source.Lookup(Index));
        }

        private ISequentialArray<T> _Source;
        private Func<T, F> _Mapping;
    }

    /// <summary>
    /// Acts as an array produced by a one to many (constant number) mapping from the source array.
    /// </summary>
    public class ExpansionSequentialArray<T, F> : ISequentialArray<F>
    {
        public ExpansionSequentialArray(ISequentialArray<T> Source, int Expansion, Func<T, IEnumerable<F>> Mapping)
        {
            this._Source = Source;
            this._Expansion = Expansion;
            this._Mapping = Mapping;
        }

        public int Count
        {
            get
            {
                return this._Expansion * this._Source.Count;
            }
        }

        public IEnumerable<KeyValuePair<int, F>> Items
        {
            get 
            {
                foreach (KeyValuePair<int, T> item in this._Source.Items)
                {
                    int i = item.Key * this._Expansion;
                    foreach (F aval in this._Mapping(item.Value))
                    {
                        yield return new KeyValuePair<int, F>(i, aval);
                        i++;
                    }
                }
            }
        }

        public IEnumerable<F> Values
        {
            get 
            {
                foreach (T val in this._Source.Values)
                {
                    foreach (F aval in this._Mapping(val))
                    {
                        yield return aval;
                    }
                }
            }
        }

        public F Lookup(int Index)
        {
            int o = Index / this._Expansion;
            int r = Index % this._Expansion;
            IEnumerable<F> items = this._Mapping(this._Source.Lookup(o));
            foreach (F item in items)
            {
                if (r > 0)
                {
                    r--;
                }
                else
                {
                    return item;
                }
            }
            return default(F); // Should never happen.
        }

        private ISequentialArray<T> _Source;
        private Func<T, IEnumerable<F>> _Mapping;
        private int _Expansion;
    }

    /// <summary>
    /// Acts as an array produced by combining two elements present in seperate arrays.
    /// </summary>
    public class ZipSequentialArray<TA, TB> : ISequentialArray<Tuple<TA, TB>>
    {
        public ZipSequentialArray(ISequentialArray<TA> SourceA, ISequentialArray<TB> SourceB)
        {
            this._SourceA = SourceA;
            this._SourceB = SourceB;
        }

        public int Count
        {
            get 
            {
                return Math.Min(this._SourceA.Count, this._SourceB.Count);
            }
        }

        public IEnumerable<KeyValuePair<int, Tuple<TA, TB>>> Items
        {
            get 
            {
                int i = 0;
                IEnumerator<TA> ae = this._SourceA.Values.GetEnumerator();
                IEnumerator<TB> be = this._SourceB.Values.GetEnumerator();
                while (ae.MoveNext() && be.MoveNext())
                {
                    yield return new KeyValuePair<int, Tuple<TA, TB>>(i, Tuple.Create(ae.Current, be.Current));
                    i++;
                }
            }
        }

        public IEnumerable<Tuple<TA, TB>> Values
        {
            get 
            {
                IEnumerator<TA> ae = this._SourceA.Values.GetEnumerator();
                IEnumerator<TB> be = this._SourceB.Values.GetEnumerator();
                while (ae.MoveNext() && be.MoveNext())
                {
                    yield return Tuple.Create(ae.Current, be.Current);
                }
            }
        }

        public Tuple<TA, TB> Lookup(int Index)
        {
            return Tuple.Create(this._SourceA.Lookup(Index), this._SourceB.Lookup(Index));
        }

        private ISequentialArray<TA> _SourceA;
        private ISequentialArray<TB> _SourceB;
    }

    /// <summary>
    /// A sequential array that acts as a range of integers.
    /// </summary>
    public struct IntRange : ISequentialArray<int>
    {
        public IntRange(int Start, int Count)
        {
            this.Start = Start;
            this.Count = Count;
        }

        int ISequentialArray<int>.Count
        {
            get 
            {
                return this.Count;
            }
        }

        public IEnumerable<KeyValuePair<int, int>> Items
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    yield return new KeyValuePair<int, int>(i, this.Start + i);
                }
            }
        }

        public IEnumerable<int> Values
        {
            get 
            {
                int end = this.Start + this.Count;
                for (int i = this.Start; i < end; i++)
                {
                    yield return i;
                }
            }
        }

        public int Lookup(int Index)
        {
            return Index + Start;
        }

        public int Start;
        public int Count; 
    }

    /// <summary>
    /// An array that will always report that it has no elements.
    /// </summary>
    public struct EmptyArray<T> : ISequentialArray<T>
    {
        public int Count
        {
            get
            {
                return 0;
            }
        }

        public IEnumerable<KeyValuePair<int, T>> Items
        {
            get 
            {
                return new KeyValuePair<int, T>[0];
            }
        }

        public IEnumerable<T> Values
        {
            get 
            {
                return new T[0];
            }
        }

        public T Lookup(int Index)
        {
            return default(T);
        }
    }
}