using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Defines an ordering of objects of the specified type.
    /// </summary>
    public interface IOrdering<T>
    {
        /// <summary>
        /// Gets if object A is greater than object B with this ordering.
        /// </summary>
        bool Greater(T A, T B);
    }

    /// <summary>
    /// Defines a grouping of objects into sets. Similar to equality comparision.
    /// </summary>
    public interface IGrouping<T>
    {
        /// <summary>
        /// Gets if the two objects belong to the same group defined by this grouping or if
        /// they are equivalent. This function is transitive.
        /// </summary>
        bool Same(T A, T B);

        /// <summary>
        /// Gets a hash code for the specified object that relates it to groups. Two objects with different
        /// hashes can not be in the same group.
        /// </summary>
        int Hash(T A);
    }

    /// <summary>
    /// An ordering defined by a delegate function.
    /// </summary>
    public struct CustomOrdering<T> : IOrdering<T>
    {
        public CustomOrdering(Func<T, T, bool> Greater)
        {
            this._Greater = Greater;
        }

        public bool Greater(T A, T B)
        {
            return this._Greater(A, B);
        }

        private Func<T, T, bool> _Greater;
    }

    /// <summary>
    /// Contains functions for sorting a set or list of data.
    /// </summary>
    public static class Sort
    {
        /// <summary>
        /// Input to the inplace sort.
        /// </summary>
        public interface IInPlaceInput<T>
        {
            /// <summary>
            /// Gets the amount of items in the array.
            /// </summary>
            int Size { get; }

            /// <summary>
            /// Gets the item at the specified index.
            /// </summary>
            T Lookup(int Index);

            /// <summary>
            /// Modifies the array so that the item at the index is the item specified.
            /// </summary>
            void Modify(int Index, T Item);
        }

        /// <summary>
        /// In place input for list.
        /// </summary>
        private struct ListInPlaceInput<T> : IInPlaceInput<T>
        {
            public ListInPlaceInput(List<T> List)
            {
                this.List = List;
            }

            public int Size
            {
                get 
                {
                    return this.List.Count;
                }
            }

            public T Lookup(int Index)
            {
                return this.List[Index];
            }

            public void Modify(int Index, T Item)
            {
                this.List[Index] = Item;
            }

            public List<T> List;
        }

        /// <summary>
        /// In place input for array.
        /// </summary>
        private struct ArrayInPlaceInput<T> : IInPlaceInput<T>
        {
            public ArrayInPlaceInput(T[] Array)
            {
                this.Array = Array;
            }

            public int Size
            {
                get 
                {
                    return this.Array.Length;
                }
            }

            public T Lookup(int Index)
            {
                return this.Array[Index];
            }

            public void Modify(int Index, T Item)
            {
                this.Array[Index] = Item;
            }

            public T[] Array;
        }

        /// <summary>
        /// Sorts the specified array of data using a generic in-place algorithim (quicksort). The comparison
        /// function returns true if the left element is greater than the right element. If the two elements are
        /// the same, any result is acceptable.
        /// </summary>
        public static void InPlace<TOrdering, TInput, TDatum>(TOrdering Ordering, TInput Input)
            where TOrdering : IOrdering<TDatum>
            where TInput : IInPlaceInput<TDatum>
        {
            InPlace<TOrdering, TInput, TDatum>(Ordering, Input, 0, Input.Size);
        }

        /// <summary>
        /// Sorts the specified region of the array of data using an in-place algorithim.
        /// </summary>
        public static void InPlace<TOrdering, TInput, TDatum>(TOrdering Ordering, TInput Input, int Start, int End)
            where TOrdering : IOrdering<TDatum>
            where TInput : IInPlaceInput<TDatum>
        {
            if (End - Start > 0)
            {
                int pivotinitial = (Start + End) / 2;
                int last = End - 1;
                TDatum pivot = Input.Lookup(pivotinitial);
                Input.Modify(pivotinitial, Input.Lookup(last));
                Input.Modify(last, pivot);

                int curstore = Start;
                for (int t = Start; t < End; t++)
                {
                    TDatum val = Input.Lookup(t);
                    if (Ordering.Greater(pivot, val))
                    {
                        if(t != curstore)
                        {
                            Input.Modify(t, Input.Lookup(curstore));
                            Input.Modify(curstore, val);
                        }
                        curstore++;
                    }
                }

                Input.Modify(last, Input.Lookup(curstore));
                Input.Modify(curstore, pivot);

                // RECURSE!
                InPlace<TOrdering, TInput, TDatum>(Ordering, Input, Start, curstore);
                InPlace<TOrdering, TInput, TDatum>(Ordering, Input, curstore + 1, End);
            }
        }

        /// <summary>
        /// Sorts a list in place.
        /// </summary>
        public static void InPlace<TOrdering, TDatum>(TOrdering Ordering, List<TDatum> List)
            where TOrdering : IOrdering<TDatum>
        {
            InPlace<TOrdering, ListInPlaceInput<TDatum>, TDatum>(Ordering, new ListInPlaceInput<TDatum>() { List = List });
        }

        /// <summary>
        /// Sorts a list in place.
        /// </summary>
        public static void InPlace<TDatum>(Func<TDatum, TDatum, bool> Greater, List<TDatum> List)
        {
            InPlace<CustomOrdering<TDatum>, TDatum>(new CustomOrdering<TDatum>(Greater), List);
        }

        /// <summary>
        /// Sorts an array in place.
        /// </summary>
        public static void InPlace<TOrdering, TDatum>(TOrdering Ordering, TDatum[] Array)
            where TOrdering : IOrdering<TDatum>
        {
            InPlace<TOrdering, ArrayInPlaceInput<TDatum>, TDatum>(Ordering, new ArrayInPlaceInput<TDatum>() { Array = Array });
        }

        /// <summary>
        /// Sorts an array in place.
        /// </summary>
        public static void InPlace<TDatum>(Func<TDatum, TDatum, bool> Greater, TDatum[] Array)
        {
            InPlace<CustomOrdering<TDatum>, TDatum>(new CustomOrdering<TDatum>(Greater), Array);
        }

        /// <summary>
        /// Sorts the specified items using a general-purpose sort (mergesort).
        /// </summary>
        public static LinkedList<T> General<T>(IOrdering<T> Ordering, IEnumerable<T> Items)
        {
            throw new NotImplementedException();
        }
    }
}