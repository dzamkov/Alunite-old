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
        public static HashSet<Triangle<Vertex>> Triangulate<Vertex>(IOrderedPolygon<Vertex> Polygon)
            where Vertex : IEquatable<Vertex>
        {
            HashSet<Triangle<Vertex>> res = new HashSet<Triangle<Vertex>>();

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

                // Go through sweeps
                LinkedListNode<_Sweep<Vertex>> cursweep = sweeps.First;
                bool startsweep = divergent; LinkedListNode<_Sweep<Vertex>> startafter = null;
                while (cursweep != null)
                {
                    LinkedListNode<_Sweep<Vertex>> nextsweep = cursweep.Next;
                    _Sweep<Vertex> curval = cursweep.Value;

                    if (curval.NextHighChain.Equals(vert))
                    {
                        startsweep = false;
                        if (divergent)
                        {
                            // There is a diagonal between a split and merge vertex. Very rare case.
                            _Sweep<Vertex> nextval = nextsweep.Value;
                            if (nextval.NextLowChain.Equals(vert))
                            {
                                res.UnionWith(curval.ProcessNextHigh(Polygon, vert, nextvert));
                                res.UnionWith(nextval.ProcessNextLow(Polygon, vert, prevvert));
                            }
                            break;
                        }
                        if (curval.NextLowChain.Equals(vert))
                        {
                            // End vertex, good bye!
                            res.UnionWith(curval.Finish(vert));
                            sweeps.Remove(cursweep);
                            if (!convergent)
                            {
                                // That sweep that just died was the result of an unfortunate merge
                                // The other sweep needs to go on though.
                                _Sweep<Vertex> nextval = nextsweep.Value;
                                res.UnionWith(nextval.ProcessNextLow(Polygon, vert, nextvert));
                            }
                            break;
                        }
                        if (convergent)
                        {
                            // Merge time
                            _Sweep<Vertex> nextval = nextsweep.Value;

                            Vertex nh = nextval.NextHighChain;
                            Vertex nl = curval.NextLowChain;

                            // Merge time
                            if (Polygon.Compare(nl, nh))
                            {
                                res.UnionWith(nextval.ProcessNextLow(Polygon, vert, nh));
                                res.UnionWith(curval.ProcessNextHigh(Polygon, vert, nh));
                            }
                            else
                            {
                                res.UnionWith(curval.ProcessNextHigh(Polygon, vert, nl));
                                res.UnionWith(nextval.ProcessNextLow(Polygon, vert, nl));
                            }
                            break;
                        }

                        if (!divergent && nextsweep != null)
                        {
                            // The next sweep might be destroyed, if it was part of a merge earlier
                            _Sweep<Vertex> nextval = nextsweep.Value;
                            if (nextval.NextLowChain.Equals(vert))
                            {
                                res.UnionWith(nextval.Finish(vert));
                                sweeps.Remove(nextsweep);
                            }
                        }
                        res.UnionWith(curval.ProcessNextHigh(Polygon, vert, prevvert));
                        
                        break;
                    }

                    if (divergent)
                    {
                        if (Polygon.Order(new Triangle<Vertex>(curval.PrevLowChain, curval.NextLowChain, vert)))
                        {
                            startafter = cursweep;
                            if (Polygon.Order(new Triangle<Vertex>(curval.NextHighChain, curval.PrevHighChain, vert)))
                            {
                                // Split!
                                _Sweep<Vertex> splitted = curval;
                                Vertex last = splitted.ReflexChain.Peek();
                                if (last.Equals(splitted.PrevHighChain))
                                {
                                    _Sweep<Vertex> created = new _Sweep<Vertex>(vert, nextvert, last);
                                    created.ProcessNextHigh(Polygon, splitted.PrevHighChain, splitted.NextHighChain);
                                    sweeps.AddAfter(cursweep, created);
                                    res.UnionWith(splitted.ProcessNextHigh(Polygon, vert, prevvert));
                                }
                                else
                                {
                                    _Sweep<Vertex> created = new _Sweep<Vertex>(vert, last, prevvert);
                                    created.ProcessNextLow(Polygon, splitted.PrevLowChain, splitted.NextLowChain);
                                    sweeps.AddBefore(cursweep, created);
                                    res.UnionWith(splitted.ProcessNextLow(Polygon, vert, nextvert));
                                }
                                startsweep = false;
                                break;
                            }
                        }
                    }

                    if (curval.NextLowChain.Equals(vert))
                    {
                        startsweep = false;
                        res.UnionWith(curval.ProcessNextLow(Polygon, vert, nextvert));
                        break;
                    }

                    cursweep = nextsweep;
                }

                // New sweep starts
                if (startsweep)
                {
                    if (startafter == null)
                    {
                        sweeps.AddFirst(new _Sweep<Vertex>(vert, nextvert, prevvert));
                    }
                    else
                    {
                        sweeps.AddAfter(startafter, new _Sweep<Vertex>(vert, nextvert, prevvert));
                    }
                }
            }

            return res;
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

            public _Sweep(Vertex Vert, Vertex Next, Vertex Prev)
            {
                this.PrevHighChain = Vert;
                this.PrevLowChain = Vert;
                this.NextHighChain = Prev;
                this.NextLowChain = Next;
                this.ReflexChain = new Stack<Vertex>();
                this.ReflexChain.Push(Vert);
            }

            public IEnumerable<Triangle<Vertex>> ProcessNextHigh(IOrderedPolygon<Vertex> Polygon, Vertex Vertex, Vertex Prev)
            {
                this.NextHighChain = Prev;
                this.PrevHighChain = Vertex;
                if (this.ReflexChain.Count < 2)
                {
                    this.ReflexChain.Push(Vertex);
                }
                else
                {
                    return this.Form(Polygon, this.PrevLowChain, Vertex, true);
                }
                return new Triangle<Vertex>[0];
            }

            public IEnumerable<Triangle<Vertex>> ProcessNextLow(IOrderedPolygon<Vertex> Polygon, Vertex Vertex, Vertex Next)
            {
                this.NextLowChain = Next;
                this.PrevLowChain = Vertex;
                if (this.ReflexChain.Count < 2)
                {
                    this.ReflexChain.Push(Vertex);
                }
                else
                {
                    return this.Form(Polygon, this.PrevHighChain, Vertex, false);
                }
                return new Triangle<Vertex>[0];
            }

            public IEnumerable<Triangle<Vertex>> Form(IOrderedPolygon<Vertex> Polygon, Vertex PreviousOppositeChain, Vertex Cur, bool High)
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
                        if (Polygon.Order(testtri))
                        {
                            yield return testtri;
                            last = next;
                        }
                        else
                        {
                            this.ReflexChain.Push(next);
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

        /// <summary>
        /// Results of a point in polygon test.
        /// </summary>
        public struct Hit
        {
            /// <summary>
            /// Relation of the point to the polygon.
            /// </summary>
            public AreaRelation Relation;

            /// <summary>
            /// If the hit is at a segment, this is the index of the segment that was hit.
            /// </summary>
            public int Segment;

            /// <summary>
            /// Length along the hit segment the point is at (if any segments were hit).
            /// </summary>
            public double Length;
        }

        /// <summary>
        /// Gets the relation between a point and a polygon with the specified segments.
        /// </summary>
        public static Hit PointTest(Point Point, IEnumerable<Segment<Point>> Segments)
        {
            bool inpoly = false;
            double lowy = double.PositiveInfinity;
            int i = 0;
            foreach (var seg in Segments)
            {
                double segxdelta = (seg.B.X - seg.A.X);
                double segydelta = (seg.B.Y - seg.A.Y);
                if (segxdelta == 0)
                {
                    if (Point.X == seg.A.X)
                    {
                        double len = (Point.Y - seg.A.Y) / segydelta;
                        if (len >= 0.0 && len < 1.0)
                        {
                            return new Hit()
                            {
                                Relation = AreaRelation.On,
                                Length = len,
                                Segment = i
                            };
                        }
                    }
                }
                else
                {

                    double segslope = segydelta / segxdelta;
                    double len = (Point.X - seg.A.X) / segxdelta;
                    if(len >= 0.0 && len < 1.0)
                    {
                        double ypos = segslope * (Point.X - seg.A.X) + seg.A.Y;
                        if (Point.Y == ypos)
                        {
                            return new Hit()
                            {
                                Relation = AreaRelation.On,
                                Length = len,
                                Segment = i
                            };
                        }
                        if (ypos < lowy && Point.Y < ypos)
                        {
                            inpoly = segxdelta < 0;
                            lowy = ypos;
                        }
                    }
                }
                i++;
            }

            return new Hit()
            {
                Relation = inpoly ? AreaRelation.Inside : AreaRelation.Outside
            };
        }
    }
}