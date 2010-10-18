using System.Collections.Generic;
using System;

namespace Alunite
{
    /// <summary>
    /// A finite, unordered collection of objects of the specified type.
    /// </summary>
    public interface ISet<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Gets the amount of items in the set.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets the items in the set (in no particular order).
        /// </summary>
        IEnumerable<T> Items { get; }
    }

    /// <summary>
    /// A set created from a collection guarnteed not to repeat values and
    /// the size of the collection.
    /// </summary>
    public class SimpleSet<T> : ISet<T>
        where T : IEquatable<T>
    {
        public SimpleSet(IEnumerable<T> Source, int Size)
        {
            this._Source = Source;
            this._Size = Size;
        }

        public int Size
        {
            get 
            {
                return this._Size;
            }
        }

        public IEnumerable<T> Items
        {
            get 
            {
                return this._Source;
            }
        }

        private IEnumerable<T> _Source;
        private int _Size;
    }

    /// <summary>
    /// Set helper functions.
    /// </summary>
    public static class Set
    {
        /// <summary>
        /// Creates a set with the specified size and items.
        /// </summary>
        public static SimpleSet<T> Create<T>(IEnumerable<T> Items, int Size)
            where T : IEquatable<T>
        {
            return new SimpleSet<T>(Items, Size);
        }

        /// <summary>
        /// Creates a shallow set from an array of items guaranteed to be distinct.
        /// </summary>
        public static SimpleSet<T> Create<T>(T[] Items)
            where T : IEquatable<T>
        {
            return new SimpleSet<T>(Items, Items.Length);
        }

        /// <summary>
        /// Creates a set from a hashset provided the hashset doesn't change.
        /// </summary>
        public static SimpleSet<T> Create<T>(HashSet<T> Items)
            where T : IEquatable<T>
        {
            return new SimpleSet<T>(Items, Items.Count);
        }

        /// <summary>
        /// Joins two collections of items into one.
        /// </summary>
        public static IEnumerable<T> Join<T>(IEnumerable<T> A, IEnumerable<T> B)
        {
            foreach (T a in A)
            {
                yield return a;
            }
            foreach (T b in B)
            {
                yield return b;
            }
        }
    }
}