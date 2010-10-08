using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains functions for sorting a set or list of data.
    /// </summary>
    public static class Sort
    {
        /// <summary>
        /// Sorts the specified array of data using a generic in-place algorithim (quicksort). The comparison
        /// function returns true if the left element is greater than the right element. If the two elements are
        /// the same, any result is acceptable.
        /// </summary>
        public static void InPlace<A, T>(A Data, Func<Tuple<T, T>, bool> Comparison)
            where A : ISequentialArray<T>, IMutableArray<T, int>
        {
            InPlace(Data, Comparison, 0, Data.Count);
        }

        /// <summary>
        /// Sorts the specified region of the array of data using an in-place algorithim.
        /// </summary>
        public static void InPlace<A, T>(A Data, Func<Tuple<T, T>, bool> Comparison, int Start, int Size)
            where A : ISequentialArray<T>, IMutableArray<T, int>
        {
            if (Size > 1)
            {
                int pivotinitial = Start + Size / 2;
                int last = Start + Size - 1;
                T pivot = Data.Lookup(pivotinitial);
                Data.Modify(pivotinitial, Data.Lookup(last));
                Data.Modify(last, pivot);

                int curstore = 0;
                for (int t = 0; t < Size - 1; t++)
                {
                    T val = Data.Lookup(t);
                    if (Comparison(new Tuple<T, T>(pivot, val)))
                    {
                        if(t != curstore)
                        {
                            Data.Modify(t, Data.Lookup(curstore));
                            Data.Modify(curstore, val);
                        }
                        curstore++;
                    }
                }

                Data.Modify(last, Data.Lookup(curstore));
                Data.Modify(curstore, pivot);

                // RECURSE!
                InPlace<A, T>(Data, Comparison, 0, curstore);
                InPlace<A, T>(Data, Comparison, curstore + 1, Size - curstore - 1);
            }
        }
    }
}