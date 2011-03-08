using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A possibly-infinite collection of items.
    /// </summary>
    public abstract class Set<T>
    {
        /// <summary>
        /// Pushes all items in this set to the specified sink.
        /// </summary>
        public abstract void Push<TSink>(TSink Sink)
            where TSink : Sink<T>;

        /// <summary>
        /// Gets all items in this set
        /// </summary>
        public virtual IEnumerable<T> Pull
        {
            get
            {
                List<T> li = new List<T>();
                this.Push<ListSink<T>>(new ListSink<T>(li));
                return li;
            }
        }

        /// <summary>
        /// Creates a set with the given filter applied.
        /// </summary>
        public virtual Set<T> Filter<TFilter>(TFilter Filter)
            where TFilter : Filter<T>
        {
            return new FilteredSet<T, TFilter>(this, Filter);
        }
    }

    /// <summary>
    /// A set defined with a collection.
    /// </summary>
    public class StaticSet<T> : Set<T>
    {
        public StaticSet(IEnumerable<T> Source)
        {
            this._Source = Source;
        }

        public override void Push<TSink>(TSink Sink)
        {
            Sink.Push(this._Source);
        }

        public override IEnumerable<T> Pull
        {
            get
            {
                return this._Source;
            }
        }

        private IEnumerable<T> _Source;
    }

    /// <summary>
    /// A set based on a source set, with a filter applied.
    /// </summary>
    public class FilteredSet<T, TFilter> : Set<T>
        where TFilter : Filter<T>
    {
        public FilteredSet(Set<T> Source, TFilter Filter)
        {
            this._Source = Source;
            this._Filter = Filter;
        }

        public override void Push<TSink>(TSink Sink)
        {
            this._Source.Push<FilteredSink<T, TFilter, TSink>>(new FilteredSink<T, TFilter, TSink>(Sink, this._Filter));
        }

        private Set<T> _Source;
        private TFilter _Filter;
    }
}