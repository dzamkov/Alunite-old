using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A finite, ordered, and possibly mutable collection of items which can be referenced by an integer.
    /// </summary>
    /// <typeparam name="T">The type of items in the array.</typeparam>
    public interface IArray<T>
    {
        /// <summary>
        /// Gets the item at the specified index. This function may throw an exception for some values, but it may
        /// not cause a modification in the array or the programs state (besides the side effects of the lookup).
        /// </summary>
        T Lookup(int Index);

        /// <summary>
        /// Gets the amount of items in the array.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets the items in the array in order by their index.
        /// </summary>
        IEnumerable<T> Items { get; }
    }

    /// <summary>
    /// An array where any element may be changed.
    /// </summary>
    public interface IMutableArray<T> : IArray<T>
    {
        /// <summary>
        /// Modifies an element in the array to reflect a value. This function may throw an exception to indicate
        /// failure.
        /// </summary>
        void Modify(int Index, T Value);
    }

    /// <summary>
    /// An array where an in-place mapping can be applied.
    /// </summary>
    public interface IMapableArray<T> : IArray<T>
    {
        /// <summary>
        /// Applies an in-place mapping, transforming all the elements in the array by the specified function.
        /// </summary>
        void Map(Func<T, T> Mapping);
    }

    /// <summary>
    /// A sequential array where items can be added at the end.
    /// </summary>
    public interface IExtendableArray<T> : IArray<T>
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
    public class StandardArray<T> : IMutableArray<T>, IMapableArray<T>
    {
        public StandardArray(T[] Items)
        {
            this._Items = Items;
        }

        public StandardArray(IArray<T> Source)
        {
            this._Items = new T[Source.Size];
            int i = 0;
            foreach (T item in Source.Items)
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

        public void Modify(int Index, T Value)
        {
            this._Items[Index] = Value;
        }

        public T Lookup(int Index)
        {
            return this._Items[Index];
        }

        public int Size
        {
            get 
            {
                return this._Items.Length;
            }
        }

        public IEnumerable<T> Items
        {
            get 
            {
                return this._Items;
            }
        }

        private T[] _Items; 
    }

    /// <summary>
    /// A numerically-indexed array where items can be added and removed quickly.
    /// </summary>
    public class ListArray<T> : IMutableArray<T>, IExtendableArray<T>, IMapableArray<T>
    {
        public ListArray()
        {
            this._Items = new List<T>();
        }

        public ListArray(IArray<T> Source)
        {
            this._Items = new List<T>(Source.Items);
        }

        public ListArray(List<T> Source)
        {
            this._Items = Source;
        }

        public ListArray(IEnumerable<T> Source)
        {
            this._Items = new List<T>(Source);
        }

        public void Modify(int Index, T Value)
        {
            this._Items[Index] = Value;
        }

        public T Lookup(int Index)
        {
            return this._Items[Index];
        }

        public int Size
        {
            get 
            {
                return this._Items.Count;
            }
        }

        public IEnumerable<T> Items
        {
            get
            {
                return this._Items;
            }
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
    public class MapArray<T, F> : IArray<F>
    {
        public MapArray(IArray<T> Source, Func<T, F> Mapping)
        {
            this._Source = Source;
            this._Mapping = Mapping;
        }

        public int Size
        {
            get 
            {
                return this._Source.Size;
            }
        }

        public IEnumerable<F> Items
        {
            get 
            {
                foreach (T val in this._Source.Items)
                {
                    yield return this._Mapping(val);
                }
            }
        }

        public F Lookup(int Index)
        {
            return this._Mapping(this._Source.Lookup(Index));
        }

        private IArray<T> _Source;
        private Func<T, F> _Mapping;
    }

    /// <summary>
    /// Acts as an array produced by a one to many (constant number) mapping from the source array.
    /// </summary>
    public class ExpansionArray<T, F> : IArray<F>
    {
        public ExpansionArray(IArray<T> Source, int Expansion, Func<T, IEnumerable<F>> Mapping)
        {
            this._Source = Source;
            this._Expansion = Expansion;
            this._Mapping = Mapping;
        }

        public int Size
        {
            get
            {
                return this._Expansion * this._Source.Size;
            }
        }

        public IEnumerable<F> Items
        {
            get 
            {
                foreach (T val in this._Source.Items)
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

        private IArray<T> _Source;
        private Func<T, IEnumerable<F>> _Mapping;
        private int _Expansion;
    }

    /// <summary>
    /// Acts as an array produced by combining two elements present in seperate arrays.
    /// </summary>
    public class ZipArray<TA, TB> : IArray<Tuple<TA, TB>>
    {
        public ZipArray(IArray<TA> SourceA, IArray<TB> SourceB)
        {
            this._SourceA = SourceA;
            this._SourceB = SourceB;
        }

        public int Size
        {
            get 
            {
                return Math.Min(this._SourceA.Size, this._SourceB.Size);
            }
        }

        public IEnumerable<Tuple<TA, TB>> Items
        {
            get 
            {
                IEnumerator<TA> ae = this._SourceA.Items.GetEnumerator();
                IEnumerator<TB> be = this._SourceB.Items.GetEnumerator();
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

        private IArray<TA> _SourceA;
        private IArray<TB> _SourceB;
    }

    /// <summary>
    /// A sequential array that acts as a range of integers.
    /// </summary>
    public struct IntRange : IArray<int>
    {
        public IntRange(int Start, int Size)
        {
            this.Start = Start;
            this.Size = Size;
        }

        int IArray<int>.Size
        {
            get 
            {
                return this.Size;
            }
        }

        public IEnumerable<int> Items
        {
            get 
            {
                int end = this.Start + this.Size;
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
        public int Size; 
    }

    /// <summary>
    /// An array that will always report that it has no elements.
    /// </summary>
    public struct EmptyArray<T> : IArray<T>
    {
        public int Size
        {
            get
            {
                return 0;
            }
        }

        public IEnumerable<T> Items
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

    /// <summary>
    /// Contains helper functions for alunite arrays.
    /// </summary>
    public static class Data
    {
        /// <summary>
        /// Creates a standard array from a source array.
        /// </summary>
        public static StandardArray<T> CreateStandard<T>(IArray<T> Source)
        {
            return new StandardArray<T>(Source);
        }

        /// <summary>
        /// Creates a standard array from a set.
        /// </summary>
        public static StandardArray<T> CreateStandard<T>(ISet<T> Source)
            where T : IEquatable<T>
        {
            return new StandardArray<T>(Source.Items, Source.Size);
        }

        /// <summary>
        /// Creates a mapped array from a source array.
        /// </summary>
        public static MapArray<T, F> Map<T, F>(IArray<T> Source, Func<T, F> Mapping)
        {
            return new MapArray<T, F>(Source, Mapping);
        }

        /// <summary>
        /// Creates an expanded array from a source array.
        /// </summary>
        public static ExpansionArray<T, F> Expand<T, F>(IArray<T> Source, int Factor, Func<T, IEnumerable<F>> Mapping)
        {
            return new ExpansionArray<T, F>(Source, Factor, Mapping);
        }

        /// <summary>
        /// Creates a zipped array from two source arrays.
        /// </summary>
        public static ZipArray<TA, TB> Zip<TA, TB>(IArray<TA> SourceA, IArray<TB> SourceB)
        {
            return new ZipArray<TA, TB>(SourceA, SourceB);
        }
    }
}