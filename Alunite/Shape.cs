using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Represents a polygon, a two-dimensional area bounded by segments. 
    /// </summary>
    /// <remarks>Segments around the polygon should have a normal that points outside the polygon. Polygons may
    /// have holes. Each vertex should be used twice by segments, as alternating endpoints.</remarks>
    /// <typeparam name="Vertex">Represents a vertex in the polygon.</typeparam>
    public interface IPolygon<Vertex>
        where Vertex : IEquatable<Vertex>
    {
        /// <summary>
        /// Gets the vertices (in an arbitrary order) that make up the polygon.
        /// </summary>
        IEnumerable<Vertex> Vertices { get; }

        /// <summary>
        /// Gets the order of the specified triangle. (Returns true if the triangle is convex, and
        /// all its segments point outwards.
        /// </summary>
        bool Order(Triangle<Vertex> Triangle);

        /// <summary>
        /// Gets a vertex X such that (Vertex, X) is a segment in the polygon.
        /// </summary>
        Vertex Next(Vertex Vertex);

        /// <summary>
        /// Gets a vertex X such that (X, Vertex) is a segment in the polygon.
        /// </summary>
        Vertex Previous(Vertex Vertex);
    }

    /// <summary>
    /// A polygon where a spatial (lexicographical) ordering of vertices can be found. 
    /// </summary>
    public interface IOrderedPolygon<Vertex> : IPolygon<Vertex>
        where Vertex : IEquatable<Vertex>
    {
        /// <summary>
        /// Gets if vertex A is lexicographically higher than vertex B.
        /// </summary>
        bool Compare(Vertex A, Vertex B);

        /// <summary>
        /// Gets the vertices (in lexicographical order) that make up the polygon.
        /// </summary>
        IEnumerable<Vertex> LexicVertices { get; }   
    }

    /// <summary>
    /// A polygon that uses Point to mark spatial positions.
    /// </summary>
    public class PointPolygon<Vertex> : IOrderedPolygon<Vertex>
        where Vertex : IEquatable<Vertex>
    {
        public PointPolygon(Func<Vertex, Point> Lookup, IEnumerable<Segment<Vertex>> Segments)
        {
            this._Lookup = Lookup;
            this._Next = new Dictionary<Vertex, Vertex>();
            this._Prev = new Dictionary<Vertex, Vertex>();
            foreach (Segment<Vertex> seg in Segments)
            {
                this._Next.Add(seg.A, seg.B);
                this._Prev.Add(seg.B, seg.A);
            }
        }

        public bool Compare(Vertex A, Vertex B)
        {
            return Point.Compare(this._Lookup(A), this._Lookup(B));
        }

        public IEnumerable<Vertex> LexicVertices
        {
            get 
            {
                List<Vertex> sorted = new List<Vertex>(this._Next.Count);
                sorted.AddRange(this.Vertices);
                Sort.InPlace<ListArray<Vertex>, Vertex>(new ListArray<Vertex>(sorted), x => this.Compare(x.A, x.B));
                return sorted;
            }
        }

        public IEnumerable<Vertex> Vertices
        {
            get 
            {
                return this._Next.Keys;
            }
        }

        public bool Order(Triangle<Vertex> Triangle)
        {
            return Alunite.Triangle.Order(new Triangle<Point>(
                this._Lookup(Triangle.A),
                this._Lookup(Triangle.B),
                this._Lookup(Triangle.C)));
        }

        public Vertex Next(Vertex Vertex)
        {
            return this._Next[Vertex];
        }

        public Vertex Previous(Vertex Vertex)
        {
            return this._Prev[Vertex];
        }

        private Func<Vertex, Point> _Lookup;
        private Dictionary<Vertex, Vertex> _Next;
        private Dictionary<Vertex, Vertex> _Prev;
    }

    /// <summary>
    /// Polygon related functions.
    /// </summary>
    public static class Polygon
    {
        /// <summary>
        /// Triangulates an ordered polygon in O(n log n) time.
        /// </summary>
        public static IEnumerable<Triangle<Vertex>> Triangulate<Vertex>(IOrderedPolygon<Vertex> Polygon)
            where Vertex : IEquatable<Vertex>
        {
            // Sweep through all vertices.
            LinkedList<_Sweep<Vertex>> sweeps = new LinkedList<_Sweep<Vertex>>();
            foreach (Vertex vert in Polygon.LexicVertices)
            {
                Vertex nextvert = Polygon.Next(vert);
                Vertex prevvert = Polygon.Previous(vert);
                bool nextvertl = Polygon.Compare(nextvert, vert);
                bool prevvertl = Polygon.Compare(prevvert, vert);
                bool divergent = nextvertl && prevvertl;
                bool convergent = !nextvertl && !prevvertl;

                if (divergent)
                {
                    // Check if it is a split
                    LinkedListNode<_Sweep<Vertex>> splitleft = null;
                    LinkedListNode<_Sweep<Vertex>> splitright = null;

                    LinkedListNode<_Sweep<Vertex>> splitleftnext = sweeps.First;
                    while (splitleftnext != null)
                    {
                        _Sweep<Vertex> splitleftval = splitleftnext.Value;
                        if (!Polygon.Order(new Triangle<Vertex>(splitleftval.PrevLowChain, splitleftval.NextLowChain, vert)))
                        {
                            break;
                        }
                        splitleft = splitleftnext;
                        splitleftnext = splitleftnext.Next;
                    }

                    LinkedListNode<_Sweep<Vertex>> splitrightnext = sweeps.Last;
                    while (splitrightnext != null)
                    {
                        _Sweep<Vertex> splitrightval = splitrightnext.Value;
                        if (!Polygon.Order(new Triangle<Vertex>(splitrightval.NextHighChain, splitrightval.PrevHighChain, vert)))
                        {
                            break;
                        }
                        splitright = splitrightnext;
                        splitrightnext = splitrightnext.Previous;
                    }

                    if (splitleft != null && splitright != null && splitleft == splitright)
                    {
                        // Split!
                        _Sweep<Vertex> splitsweep = splitleft.Value;
                        _Sweep<Vertex> splitted = splitsweep;
                        Vertex last = splitted.ReflexChain.Peek();
                        if (last.Equals(splitted.PrevHighChain))
                        {
                            _Sweep<Vertex> created = new _Sweep<Vertex>(vert, nextvert, last, Polygon);
                            created.ProcessNextHigh(splitted.PrevHighChain, splitted.NextHighChain);
                            sweeps.AddAfter(splitleft, created);
                            foreach (Triangle<Vertex> tri in splitted.ProcessNextHigh(vert, prevvert))
                            {
                                yield return tri;
                            }
                        }
                        else
                        {
                            _Sweep<Vertex> created = new _Sweep<Vertex>(vert, last, prevvert, Polygon);
                            created.ProcessNextLow(splitted.PrevLowChain, splitted.NextLowChain);
                            sweeps.AddBefore(splitleft, created);
                            foreach (Triangle<Vertex> tri in splitted.ProcessNextLow(vert, nextvert))
                            {
                                yield return tri;
                            }
                        }
                    }
                    else
                    {
                        // Not a split, add a new sweep
                        if (splitleft == null)
                        {
                            sweeps.AddFirst(new _Sweep<Vertex>(vert, nextvert, prevvert, Polygon));
                        }
                        else
                        {
                            if (splitright == null)
                            {
                                sweeps.AddLast(new _Sweep<Vertex>(vert, nextvert, prevvert, Polygon));
                            }
                            else
                            {
                                sweeps.AddAfter(splitleft, new _Sweep<Vertex>(vert, nextvert, prevvert, Polygon));
                            }
                        }
                    }
                    continue;
                }

                if (convergent)
                {
                    // Some poor sweep is gonna get destroyed.
                    // Lets see who it is.
                    LinkedListNode<_Sweep<Vertex>> poorsweep = sweeps.First;
                    while (true)
                    {
                        _Sweep<Vertex> sweep = poorsweep.Value;
                        if (sweep.NextHighChain.Equals(vert) && sweep.NextLowChain.Equals(vert))
                        {
                            foreach (Triangle<Vertex> tri in sweep.Finish(vert))
                            {
                                yield return tri;
                            }

                            // Good bye!
                            sweeps.Remove(poorsweep);

                            break;
                        }
                        poorsweep = poorsweep.Next;
                    }
                    continue;
                }
                

                // Not divergent or convergent, that means it belongs to one of the sweeps.
                foreach (_Sweep<Vertex> sweep in sweeps)
                {
                    if (sweep.NextLowChain.Equals(vert))
                    {
                        foreach (Triangle<Vertex> tri in sweep.ProcessNextLow(vert, nextvert))
                        {
                            yield return tri;
                        }
                        break;
                    }
                    if (sweep.NextHighChain.Equals(vert))
                    {
                        foreach (Triangle<Vertex> tri in sweep.ProcessNextHigh(vert, prevvert))
                        {
                            yield return tri;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// The current state of a monotone sweep in a triangulation.
        /// </summary>
        private class _Sweep<Vertex>
            where Vertex : IEquatable<Vertex>
        {
            public _Sweep()
            {
                this.ReflexChain = new Stack<Vertex>();
            }

            public _Sweep(Vertex Vert, Vertex Next, Vertex Prev, IOrderedPolygon<Vertex> Polygon)
            {
                this.PrevHighChain = Vert;
                this.PrevLowChain = Vert;
                this.NextHighChain = Prev;
                this.NextLowChain = Next;
                this.ReflexChain = new Stack<Vertex>();
                this.ReflexChain.Push(Vert);
                this.Polygon = Polygon;
            }

            public IEnumerable<Triangle<Vertex>> ProcessNextHigh(Vertex Vertex, Vertex Prev)
            {
                this.NextHighChain = Prev;
                this.PrevHighChain = Vertex;
                if (this.ReflexChain.Count < 2)
                {
                    this.ReflexChain.Push(Vertex);
                }
                else
                {
                    return this.Form(this.PrevLowChain, Vertex, true);
                }
                return new Triangle<Vertex>[0];
            }

            public IEnumerable<Triangle<Vertex>> ProcessNextLow(Vertex Vertex, Vertex Next)
            {
                this.NextLowChain = Next;
                this.PrevLowChain = Vertex;
                if (this.ReflexChain.Count < 2)
                {
                    this.ReflexChain.Push(Vertex);
                }
                else
                {
                    return this.Form(this.PrevHighChain, Vertex, false);
                }
                return new Triangle<Vertex>[0];
            }

            public IEnumerable<Triangle<Vertex>> Form(Vertex PreviousOppositeChain, Vertex Cur, bool High)
            {
                Vertex top = this.ReflexChain.Pop();
                if (top.Equals(PreviousOppositeChain))
                {
                    Vertex last = top;
                    while (this.ReflexChain.Count > 0)
                    {
                        Vertex next = this.ReflexChain.Pop();
                        if (High)
                        {
                            yield return new Triangle<Vertex>(Cur, next, last);
                        }
                        else
                        {
                            yield return new Triangle<Vertex>(Cur, last, next);
                        }
                        last = next;
                    }
                    this.ReflexChain.Push(top);
                    this.ReflexChain.Push(Cur);
                }
                else
                {
                    Vertex last = top;
                    while (this.ReflexChain.Count > 0)
                    {
                        Vertex next = this.ReflexChain.Pop();
                        Triangle<Vertex> testtri = High ? new Triangle<Vertex>(Cur, last, next) : new Triangle<Vertex>(Cur, next, last);
                        if (this.Polygon.Order(testtri))
                        {
                            yield return testtri;
                            last = next;
                        }
                        else
                        {
                            break;
                        }
                    }
                    this.ReflexChain.Push(last);
                    this.ReflexChain.Push(Cur);
                }
            }

            public IEnumerable<Triangle<Vertex>> Finish(Vertex Last)
            {
                Vertex top = this.ReflexChain.Pop();
                bool high = top.Equals(this.PrevHighChain);
                Vertex last = top;
                while (this.ReflexChain.Count > 0)
                {
                    Vertex next = this.ReflexChain.Pop();
                    if (high)
                    {
                        yield return new Triangle<Vertex>(Last, last, next);
                    }
                    else
                    {
                        yield return new Triangle<Vertex>(Last, next, last);
                    }
                    last = next;
                }
            }

            public Vertex PrevLowChain;
            public Vertex PrevHighChain;
            public Vertex NextLowChain;
            public Vertex NextHighChain;
            public IOrderedPolygon<Vertex> Polygon;
            public Stack<Vertex> ReflexChain;
        }

        /// <summary>
        /// Gets the next node in a list, wrapping to the first item if the end is reached.
        /// </summary>
        private static LinkedListNode<T> _Next<T>(LinkedList<T> List, LinkedListNode<T> Node)
        {
            return Node.Next ?? List.First;
        }

        /// <summary>
        /// Gets the segments that bound a polygon created from a counter-clockwise collection of points.
        /// </summary>
        public static IEnumerable<Segment<Vertex>> Segments<Vertex>(IEnumerable<Vertex> Vertices)
            where Vertex : IEquatable<Vertex>
        {
            IEnumerator<Vertex> enumerator = Vertices.GetEnumerator();
            if (enumerator.MoveNext())
            {
                Vertex first = enumerator.Current;
                Vertex prev = first;
                while (enumerator.MoveNext())
                {
                    Vertex cur = enumerator.Current;
                    yield return new Segment<Vertex>(prev, cur);
                    prev = cur;
                }
                yield return new Segment<Vertex>(prev, first);
            }
        }

        /// <summary>
        /// Gets the segments that bound a polygon created from a counter-clockwise collection of numerical points.
        /// </summary>
        public static IEnumerable<Segment<int>> Segments(int Amount)
        {
            for (int t = 0; t < Amount - 1; t++)
            {
                yield return new Segment<int>(t, t + 1);
            }
            yield return new Segment<int>(Amount - 1, 0);
        }
    }
}