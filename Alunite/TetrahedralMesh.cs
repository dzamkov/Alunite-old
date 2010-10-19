using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Describes a set of referencable tetrahedra that use vertices of a specified type.
    /// </summary>
    /// <typeparam name="Tetrahedron">A reference to a tetrahedron.</typeparam>
    /// <typeparam name="Vertex">A reference to a vertex.</typeparam>
    public interface ITetrahedralMesh<Tetrahedron, Vertex>
        where Tetrahedron : struct, IEquatable<Tetrahedron>
        where Vertex : struct, IEquatable<Vertex>
    {
        /// <summary>
        /// Looks up the tetrahedron with the specified reference and returns the vertices it
        /// uses as tetrahedron data.
        /// </summary>
        Tetrahedron<Vertex> Lookup(Tetrahedron Tetrahedron);

        /// <summary>
        /// Gets a reference to the tetrahedron with the specified vertices, if it is in the mesh.
        /// </summary>
        Tetrahedron? Lookup(Tetrahedron<Vertex> Tetrahedron);

        /// <summary>
        /// Gets the set of tetrahedra in the mesh.
        /// </summary>
        ISet<Tetrahedron> Tetrahedra { get; }
    }

    /// <summary>
    /// A simple tetrahedral mesh that stores tetrahedra and boundaries.
    /// </summary>
    /// <typeparam name="T">Type that represents a point.</typeparam>
    public class TetrahedralMesh<T> : ITetrahedralMesh<Tetrahedron<T>, T>
        where T : struct, IEquatable<T>
    {
        public TetrahedralMesh()
        {
            this._Tetrahedra = new HashSet<Tetrahedron<T>>();
            this._Boundaries = new Dictionary<Triangle<T>, Tetrahedron<T>>();
            this._Interiors = new Dictionary<Triangle<T>, Tetrahedron<T>>();
        }

        private TetrahedralMesh(
            HashSet<Tetrahedron<T>> Tetrahedra,
            Dictionary<Triangle<T>, Tetrahedron<T>> Boundaries,
            Dictionary<Triangle<T>, Tetrahedron<T>> Interiors)
        {
            this._Tetrahedra = Tetrahedra;
            this._Boundaries = Boundaries;
            this._Interiors = Interiors;
        }

        /// <summary>
        /// Creates a tetrahedral mesh from this mesh based on the specified point mapping with the requirement
        /// that the mapping function is one to one.
        /// </summary>
        public TetrahedralMesh<F> Map<F>(Func<T, F> Mapping)
            where F : struct, IEquatable<F>
        {
            HashSet<Tetrahedron<F>> newtetras = new HashSet<Tetrahedron<F>>();
            Dictionary<Triangle<F>, Tetrahedron<F>> newbounds = new Dictionary<Triangle<F>, Tetrahedron<F>>();
            Dictionary<Triangle<F>, Tetrahedron<F>> newinteriors = new Dictionary<Triangle<F>, Tetrahedron<F>>();
            foreach (Tetrahedron<T> tet in this._Tetrahedra)
            {
                newtetras.Add(new Tetrahedron<F>(
                    Mapping(tet.A),
                    Mapping(tet.B),
                    Mapping(tet.C),
                    Mapping(tet.D)));
            }
            foreach (KeyValuePair<Triangle<T>, Tetrahedron<T>> bound in this._Boundaries)
            {
                newbounds.Add(
                    new Triangle<F>(
                        Mapping(bound.Key.A),
                        Mapping(bound.Key.B),
                        Mapping(bound.Key.C)),
                    new Tetrahedron<F>(
                        Mapping(bound.Value.A),
                        Mapping(bound.Value.B),
                        Mapping(bound.Value.C),
                        Mapping(bound.Value.D)));
            }
            foreach (KeyValuePair<Triangle<T>, Tetrahedron<T>> interior in this._Interiors)
            {
                newinteriors.Add(
                    new Triangle<F>(
                        Mapping(interior.Key.A),
                        Mapping(interior.Key.B),
                        Mapping(interior.Key.C)),
                    new Tetrahedron<F>(
                        Mapping(interior.Value.A),
                        Mapping(interior.Value.B),
                        Mapping(interior.Value.C),
                        Mapping(interior.Value.D)));
            }
            return new TetrahedralMesh<F>(newtetras, newbounds, newinteriors);
        }

        /// <summary>
        /// Tries adding a tetrahedron to the tetrahedral mesh. If this will result in a conflict, the tetrahedron
        /// is not added and false is returned.
        /// </summary>
        public bool Add(Tetrahedron<T> Tetrahedron)
        {
            if (this.CanAdd(Tetrahedron))
            {
                this.AddUnchecked(Tetrahedron);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Forces the specified tetrahedron to be added, causing the validity of the structure to
        /// fail if a conflict with another tetrahedron results.
        /// </summary>
        public void AddUnchecked(Tetrahedron<T> Tetrahedron)
        {
            foreach (Triangle<T> face in Tetrahedron.Faces)
            {
                Tetrahedron<T> bound;

                if (this._Boundaries.TryGetValue(face.Flip, out bound))
                {
                    this._Interiors.Add(face.Flip, bound);
                    this._Interiors.Add(face, Tetrahedron);
                    this._Boundaries.Remove(face.Flip);
                }
                else
                {
                    this._Boundaries.Add(face, Tetrahedron);
                }
            }
            this._Tetrahedra.Add(Tetrahedron);
        }

        /// <summary>
        /// Gets if the specified tetrahedron can be added to the mesh without conflict.
        /// </summary>
        public bool CanAdd(Tetrahedron<T> Tetrahedron)
        {
            foreach (Triangle<T> face in Tetrahedron.Faces)
            {
                if (this._Interiors.ContainsKey(face) || this._Boundaries.ContainsKey(face))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Removes a tetrahedron from the tetrahedral mesh.
        /// </summary>
        public bool Remove(Tetrahedron<T> Tetrahedron)
        {
            if (this._Tetrahedra.Remove(Tetrahedron))
            {
                foreach (Triangle<T> face in Tetrahedron.Faces)
                {
                    if (this._Interiors.ContainsKey(face))
                    {
                        this._Boundaries.Add(face.Flip, this._Interiors[face.Flip]);
                        this._Interiors.Remove(face);
                        this._Interiors.Remove(face.Flip);
                    }
                    else
                    {
                        this._Boundaries.Remove(face);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Merges the pentahedron defined by A and B (the area between them should be divided into three tetrahedra in
        /// the mesh, each tetrahedra sharing two points on the base and the vertices of A and B) into two tetrahedra. If merging the pentahedron
        /// will result in a conflict with other tetrahedra, no change is made and false is returned.
        /// </summary>
        public bool MergePentahedron(Tetrahedron<T> A, Tetrahedron<T> B)
        {
            Triangle<T> b = A.Base;
            Tetrahedron<T> olda = new Tetrahedron<T>(B.Vertex, A.Vertex, b.A, b.B);
            Tetrahedron<T> oldb = new Tetrahedron<T>(B.Vertex, A.Vertex, b.B, b.C);
            Tetrahedron<T> oldc = new Tetrahedron<T>(B.Vertex, A.Vertex, b.C, b.A);
            this.Remove(olda);
            this.Remove(oldb);
            this.Remove(oldc);
            if (!this.Add(A))
            {
                this.Add(olda);
                this.Add(oldb);
                this.Add(oldc);
                return false;
            }
            if (!this.Add(B))
            {
                this.Remove(A);
                this.Add(olda);
                this.Add(oldb);
                this.Add(oldc);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Splits the two tetrahedra sharing a common base into three tetrahedra. If splitting the pentahedron
        /// they create will result in a conflict with other tetrahedra, no change is made and false is returned.
        /// </summary>
        public bool SplitPentahedron(Tetrahedron<T> A, Tetrahedron<T> B)
        {
            Triangle<T> b = A.Base;
            Tetrahedron<T> newa = new Tetrahedron<T>(b.A, b.B, B.Vertex, A.Vertex);
            Tetrahedron<T> newb = new Tetrahedron<T>(b.B, b.C, B.Vertex, A.Vertex); // hehe
            Tetrahedron<T> newc = new Tetrahedron<T>(b.C, b.A, B.Vertex, A.Vertex);
            this.Remove(A);
            this.Remove(B);
            if (!this.Add(newa))
            {
                this.Add(A);
                this.Add(B);
                return false;
            }
            if (!this.Add(newb))
            {
                this.Remove(newa);
                this.Add(A);
                this.Add(B);
                return false;
            }
            if (!this.Add(newc))
            {
                this.Remove(newb);
                this.Remove(newa);
                this.Add(A);
                this.Add(B);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the tetrahedron that has the specified interior face.
        /// </summary>
        public Tetrahedron<T>? GetInterior(Triangle<T> Face)
        {
            Tetrahedron<T> tetra;
            if (this._Interiors.TryGetValue(Face, out tetra))
            {
                return tetra;
            }
            return null;
        }

        /// <summary>
        /// Gets if the mesh contains the specified tetrahedron.
        /// </summary>
        public bool Contains(Tetrahedron<T> Tetrahedron)
        {
            return this._Tetrahedra.Contains(Tetrahedron);
        }

        public Tetrahedron<T> Lookup(Tetrahedron<T> Tetrahedron)
        {
            return Tetrahedron;
        }

        Tetrahedron<T>? ITetrahedralMesh<Tetrahedron<T>, T>.Lookup(Tetrahedron<T> Tetrahedron)
        {
            if (this._Tetrahedra.Contains(Tetrahedron))
            {
                return Tetrahedron;
            }
            else
            {
                return null;
            }
        }

        public ISet<Tetrahedron<T>> Tetrahedra
        {
            get
            {
                return new SimpleSet<Tetrahedron<T>>(this._Tetrahedra, this._Tetrahedra.Count);
            }
        }

        /// <summary>
        /// Gets all the boundaries of the mesh, along with the tetrahedra the boundaries are on.
        /// </summary>
        public IEnumerable<KeyValuePair<Triangle<T>, Tetrahedron<T>>> TetrahedraBoundary
        {
            get
            {
                return this._Boundaries;
            }
        }

        /// <summary>
        /// Gets the triangles that form the boundaries of this tetrahedral mesh.
        /// </summary>
        public ISet<Triangle<T>> Boundary
        {
            get
            {
                return new SimpleSet<Triangle<T>>(this._Boundaries.Keys, this._Boundaries.Count);
            }
        }



        private HashSet<Tetrahedron<T>> _Tetrahedra;
        private Dictionary<Triangle<T>, Tetrahedron<T>> _Boundaries;
        private Dictionary<Triangle<T>, Tetrahedron<T>> _Interiors;
    }
}