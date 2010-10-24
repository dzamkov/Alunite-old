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
                        _Sweep<Vertex> nlow = splitsweep;
                        _Sweep<Vertex> nhigh = new _Sweep<Vertex>(); sweeps.AddAfter(splitleft, nhigh);
                        nhigh.PrevHighChain = nlow.PrevHighChain;
                        nhigh.NextHighChain = nlow.NextHighChain;
                        nhigh.PrevLowChain = vert;
                        nhigh.NextLowChain = nextvert;
                        nlow.PrevHighChain = vert;
                        nlow.NextHighChain = prevvert;
                    }
                    else
                    {
                        // Not a split, add a new sweep
                        if (splitleft == null)
                        {
                            sweeps.AddFirst(new _Sweep<Vertex>(vert, nextvert, prevvert));
                        }
                        else
                        {
                            if (splitright == null)
                            {
                                sweeps.AddLast(new _Sweep<Vertex>(vert, nextvert, prevvert));
                            }
                            else
                            {
                                sweeps.AddAfter(splitleft, new _Sweep<Vertex>(vert, nextvert, prevvert));
                            }
                        }
                    }

                    continue;
                }

                if (convergent)
                {
                    // AWW SNAP!!, some poor sweep is gonna get destroyed.
                    // Lets see who it is.

                    LinkedListNode<_Sweep<Vertex>> poorsweep = sweeps.First;
                    while (true)
                    {
                        _Sweep<Vertex> sweep = poorsweep.Value;
                        if (sweep.NextHighChain.Equals(vert) && sweep.NextLowChain.Equals(vert))
                        {
                            break;
                        }
                        poorsweep = poorsweep.Next;
                    }

                    // Good bye!

                    sweeps.Remove(poorsweep);
                }
                

                // Not divergent or convergent, that means it belongs to one of the sweeps.
                foreach (_Sweep<Vertex> sweep in sweeps)
                {
                    if (sweep.NextLowChain.Equals(vert))
                    {
                        sweep.PrevLowChain = vert;
                        sweep.NextLowChain = nextvert;
                    }
                    if (sweep.NextHighChain.Equals(vert))
                    {
                        sweep.PrevHighChain = vert;
                        sweep.NextHighChain = prevvert;
                    }
                }

            }

            return null;
        }

        /// <summary>
        /// The current state of a monotone sweep in a triangulation.
        /// </summary>
        private class _Sweep<Vertex>
            where Vertex : IEquatable<Vertex>
        {
            public _Sweep()
            {

            }

            public _Sweep(Vertex Vert, Vertex Next, Vertex Prev)
            {
                this.PrevHighChain = Vert;
                this.PrevLowChain = Vert;
                this.NextHighChain = Prev;
                this.NextLowChain = Next;
            }

            public Vertex PrevLowChain;
            public Vertex PrevHighChain;
            public Vertex NextLowChain;
            public Vertex NextHighChain;
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