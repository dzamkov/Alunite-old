using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Maintains a collection of objects that contain a certain resource. This can be used to reference one of the using objects
    /// directly when it is identical to the target object. The set weakly references target objects and will sort them by likeliness
    /// of being accepted.
    /// </summary>
    public class UsageSet<TUsage>
    {
        public UsageSet()
        {
            this._Set = new LinkedList<WeakReference>();
        }

        public UsageSet(IEnumerable<TUsage> Usages)
        {
            this._Set = new LinkedList<WeakReference>(
                from u in Usages
                select new WeakReference(u, false));
        }

        /// <summary>
        /// Gets the indices to the usages in the usage set.
        /// </summary>
        public IEnumerable<Index> Usages
        {
            get
            {
                LinkedListNode<WeakReference> cur = this._Set.First;
                while (cur != null)
                {
                    LinkedListNode<WeakReference> next = cur.Next;
                    WeakReference val = cur.Value;
                    if (val.IsAlive)
                    {
                        yield return new Index()
                        {
                            _Node = cur,
                            _Value = (TUsage)val.Target
                        };
                    }
                    else
                    {
                        this._Set.Remove(cur);
                    }
                    cur = next;
                }
            }
        }

        /// <summary>
        /// Adds (or rather, makes informed) a usage to the usage set.
        /// </summary>
        public Index Add(TUsage Usage)
        {
            return new Index()
            {
                _Value = Usage,
                _Node = this._Set.AddFirst(new WeakReference(Usage, false))
            };
        }

        /// <summary>
        /// Informs the usage set that the usage at the specified index has been accepted as useful. This allows the
        /// usage set to reorder usages by usefulness as needed.
        /// </summary>
        public void Accept(Index Index)
        {
            this._Set.Remove(Index._Node);
            this._Set.AddFirst(Index._Node);
        }

        /// <summary>
        /// An index to a particular usage in the usage set.
        /// </summary>
        public struct Index
        {
            /// <summary>
            /// Gets the value of the usage set at this index.
            /// </summary>
            public TUsage Value
            {
                get
                {
                    return this._Value;
                }
            }

            internal TUsage _Value;
            internal LinkedListNode<WeakReference> _Node;
        }

        private LinkedList<WeakReference> _Set;
    }
}