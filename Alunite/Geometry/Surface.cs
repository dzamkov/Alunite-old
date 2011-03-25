using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Defines a mapping of values on a three-dimensional surface. The surface is directed (has a front and back face) and
    /// can be open or closed.
    /// </summary>
    public abstract class Surface<T> : Data<Surface<T>>
    {
        /// <summary>
        /// Gets information about all front-face points hit by the given directed segment. The hits will be returned in ascending
        /// order of length.
        /// </summary>
        public abstract IEnumerable<SurfaceHit<T>> Trace(Segment<Vector> Segment);

        /// <summary>
        /// Approximates this surface with a mesh using the specified integer as a guide for the target amount
        /// of triangles the mesh should have.
        /// </summary>
        public virtual Surface<T> ApproximateMesh(int Triangles)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A surface with no data associated with points.
    /// </summary>
    public abstract class Foil : Surface<Void>
    {

    }

    /// <summary>
    /// A surface defined by a collection of triangles.
    /// </summary>
    /// <typeparam name="T">The type of a value at any vertex on a triangle.</typeparam>
    /// <typeparam name="TTriangle">A reference to a triangle within the mesh.</typeparam>
    /// <typeparam name="TPoint">A reference to a point within the mesh.</typeparam>
    public abstract class Mesh<T, TTriangle, TVertex> : Surface<T>
    {
        /// <summary>
        /// Gets the location of the given point.
        /// </summary>
        public abstract Vector LookupVertex(TVertex Vertex);

        /// <summary>
        /// Gets the points in the given triangle.
        /// </summary>
        public abstract Triangle<TVertex> LookupTriangle(TTriangle Triangle);

        /// <summary>
        /// Gets the surface value for the triangle at the given uv coordinates.
        /// </summary>
        public abstract T GetValue(TTriangle Triangle, Point UV);

        /// <summary>
        /// Gets all the triangles in the mesh.
        /// </summary>
        public abstract IEnumerable<TTriangle> Triangles { get; }

        public override IEnumerable<SurfaceHit<T>> Trace(Segment<Vector> Segment)
        {
            // Brute force
            LinkedList<SurfaceHit<T>> hits = new LinkedList<SurfaceHit<T>>();
            foreach (TTriangle tri in this.Triangles)
            {
                Triangle<TVertex> dtri = this.LookupTriangle(tri);
                Triangle<Vector> vtri = new Triangle<Vector>(
                    this.LookupVertex(dtri.A),
                    this.LookupVertex(dtri.B),
                    this.LookupVertex(dtri.C));
                double len;
                Vector pos;
                Point uv;
                if (Triangle.Intersect(vtri, Segment, out len, out pos, out uv))
                {
                    T val = this.GetValue(tri, uv);
                    SurfaceHit<T> hit = new SurfaceHit<T>(val, len, pos, Triangle.Normal(vtri));
                    LinkedListNode<SurfaceHit<T>> insertafter = null;
                    LinkedListNode<SurfaceHit<T>> cur = hits.First;
                    while (cur != null)
                    {
                        if (len < cur.Value.Length)
                        {
                            break;
                        }
                    }
                    if (insertafter == null)
                    {
                        hits.AddFirst(hit);
                    }
                    else
                    {
                        hits.AddAfter(insertafter, hit);
                    }
                }
            }
            return hits;
        }
    }

    /// <summary>
    /// A mesh defined by a list of vertices and a list of triangles with no additional spatial organization.
    /// </summary>
    public sealed class ListMesh : Mesh<Void, int, int>
    {
        public ListMesh(List<Vector> Vertices, List<Triangle<int>> Triangles)
        {
            this._Vertices = Vertices;
            this._Triangles = Triangles;
        }

        public override Void GetValue(int Triangle, Point UV)
        {
            return Void.Value;
        }

        public override Vector LookupVertex(int Vertex)
        {
            return this._Vertices[Vertex];
        }

        public override Triangle<int> LookupTriangle(int Triangle)
        {
            return this._Triangles[Triangle];
        }

        public override IEnumerable<int> Triangles
        {
            get
            {
                return Enumerable.Range(0, this._Triangles.Count);
            }
        }

        private List<Vector> _Vertices;
        private List<Triangle<int>> _Triangles;
    }

    /// <summary>
    /// Represents a front-face hit on a surface by a directed segment.
    /// </summary>
    public struct SurfaceHit<T>
    {
        public SurfaceHit(T Value, double Length, Vector Offset, Vector Normal)
        {
            this.Value = Value;
            this.Length = Length;
            this.Offset = Offset;
            this.Normal = Normal;
        }

        public SurfaceHit(T Value, double Length, Vector Offset)
        {
            this.Value = Value;
            this.Length = Length;
            this.Offset = Offset;
            this.Normal = Vector.Zero;
        }

        /// <summary>
        /// The value of the surface on the point that was hit.
        /// </summary>
        public T Value;

        /// <summary>
        /// The length, in relation to the segment, where the hit is.
        /// </summary>
        public double Length;

        /// <summary>
        /// The offset of the hit from the surface.
        /// </summary>
        public Vector Offset;

        /// <summary>
        /// The normal of the hit, or a zero vector if the surface is discontinous at the hit point.
        /// </summary>
        public Vector Normal;
    }
}