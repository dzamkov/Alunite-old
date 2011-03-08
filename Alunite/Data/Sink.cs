using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An object to which an unbounded amount of items can be pushed.
    /// </summary>
    /// <typeparam name="T">The type of items this sink can receive.</typeparam>
    public abstract class Sink<T>
    {
        /// <summary>
        /// Pushes, or gives, an item to this sink.
        /// </summary>
        public abstract void Push(T Item);

        /// <summary>
        /// Pushes an ordered collection of items to the sink.
        /// </summary>
        public void Push(IEnumerable<T> Items)
        {
            foreach (T item in Items)
            {
                this.Push(item);
            }
        }

        /// <summary>
        /// Creates a sink where all pushed items are appended to a list.
        /// </summary>
        public static ListSink<T> ToList(List<T> List)
        {
            return new ListSink<T>(List);
        }
    }

    /// <summary>
    /// A sink where all pushed items are appeneded to a list.
    /// </summary>
    public sealed class ListSink<T> : Sink<T>
    {
        public ListSink(List<T> Target)
        {
            this._Target = Target;
        }

        /// <summary>
        /// Gets the target list of this sink.
        /// </summary>
        public List<T> Target
        {
            get
            {
                return this._Target;
            }
        }

        public override void Push(T Item)
        {
            this._Target.Add(Item);
        }

        private List<T> _Target;
    }

    /// <summary>
    /// A sink that filters pushed items before passing them on to another sink.
    /// </summary>
    public sealed class FilteredSink<T, TFilter, TTarget> : Sink<T>
        where TFilter : Filter<T>
        where TTarget : Sink<T>
    {
        public FilteredSink(TTarget Target, TFilter Filter)
        {
            this._Target = Target;
            this._Filter = Filter;
        }

        public override void Push(T Item)
        {
            if (this._Filter.Allow(Item))
            {
                this._Target.Push(Item);
            }
        }

        private TTarget _Target;
        private TFilter _Filter;
    }
}