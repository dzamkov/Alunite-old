using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Polygon related functions.
    /// </summary>
    public static class Polygon
    {
        /// <summary>
        /// Creates a triangulation of a polygon from a linked list (that is destroyed in the process). The specified
        /// valid predicate is used to determine if a triangle can be in the final triangulation (right order, no points inside). 
        /// The triangulation has no special properties. Note that if the polygon is monotone, the
        /// valid function need not check if there are any polygon points inside the triangle and the algorithim becomes O(n).
        /// </summary>
        public static IEnumerable<Triangle<T>> Triangulate<T>(LinkedList<T> Polygon, Predicate<Triangle<T>> Valid)
            where T : IEquatable<T>
        {
            List<Triangle<T>> tris = new List<Triangle<T>>();
            LinkedListNode<T> cur = Polygon.First;
            LinkedListNode<T> next = cur.Next;
            LinkedListNode<T> after = next.Next;
            while (Polygon.Count > 2)
            {
                Triangle<T> testtri = new Triangle<T>(cur.Value, next.Value, after.Value);
                if (Valid(testtri))
                {
                    tris.Add(testtri);
                    Polygon.Remove(next);
                    next = after;
                    after = _Next(Polygon, next);
                }
                else
                {
                    cur = next;
                    next = after;
                    after = _Next(Polygon, next);
                }
            }
            return tris;
        }

        /// <summary>
        /// Gets the next node in a list, wrapping to the first item if the end is reached.
        /// </summary>
        private static LinkedListNode<T> _Next<T>(LinkedList<T> List, LinkedListNode<T> Node)
        {
            return Node.Next ?? List.First;
        }
    }
}