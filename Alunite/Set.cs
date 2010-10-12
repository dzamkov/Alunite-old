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

    }
}