using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An infinite grid of regularly-spaced vertices.
    /// </summary>
    public class Lattice : IArray<Vector, IVector>
    {
        public Lattice(Vector Origin, Vector UnitSize)
        {
            this._Origin = Origin;
            this._UnitSize = UnitSize;
        }

        public Vector Lookup(IVector Index)
        {
            return this._Origin + Vector.Scale(this._UnitSize, Index);
        }

        private Vector _Origin;
        private Vector _UnitSize;
    }

    /// <summary>
    /// A finite area on a lattice.
    /// </summary>
    public class Grid : ISequentialArray<Vector>, IFiniteArray<Vector, IVector>
    {
        public Grid(Lattice Source, IVector Size)
        {
            this._Source = Source;
            this._Size = Size;
        }

        public Vector Lookup(IVector Index)
        {
            return this._Source.Lookup(Index);
        }

        public Vector Lookup(int Index)
        {
            return this._Source.Lookup(this.Position(Index));
        }

        /// <summary>
        /// Gets the position of the vertex at the specified index.
        /// </summary>
        public IVector Position(int Index)
        {
            int x = Index % this._Size.X;
            Index /= this._Size.X;
            int y = Index % this._Size.Y;
            Index /= this._Size.Y;
            int z = Index;
            return new IVector(x, y, z);
        }

        /// <summary>
        /// Gets the index for the vertex at the specified position.
        /// </summary>
        public int Index(IVector Position)
        {
            return Position.X + (Position.Y * this._Size.X) + (Position.Z * this._Size.X * this._Size.Y);
        }

        public IEnumerable<KeyValuePair<int, Vector>> Items
        {
            get 
            {
                int i = 0;
                for (int z = 0; z < this._Size.Z; z++)
                {
                    for (int y = 0; y < this._Size.Y; y++)
                    {
                        for (int x = 0; x < this._Size.X; x++)
                        {
                            yield return new KeyValuePair<int, Vector>(i, this._Source.Lookup(new IVector(x, y, z)));
                            i++;
                        }
                    }
                }
            }
        }

        public Vector Default
        {
            get
            {
                return new Vector();
            }
        }

        IEnumerable<KeyValuePair<IVector, Vector>> IFiniteArray<Vector, IVector>.Items
        {
            get
            {
                for (int z = 0; z < this._Size.Z; z++)
                {
                    for (int y = 0; y < this._Size.Y; y++)
                    {
                        for (int x = 0; x < this._Size.X; x++)
                        {
                            IVector vec = new IVector(x, y, z);
                            yield return new KeyValuePair<IVector, Vector>(vec, this._Source.Lookup(vec));
                        }
                    }
                }
            }
        }

        public IEnumerable<Vector> Values
        {
            get 
            {
                for (int z = 0; z < this._Size.Z; z++)
                {
                    for (int y = 0; y < this._Size.Y; y++)
                    {
                        for (int x = 0; x < this._Size.X; x++)
                        {
                            yield return this._Source.Lookup(new IVector(x, y, z));
                        }
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return this._Size.X * this._Size.Y * this._Size.Z;
            }
        }

        /// <summary>
        /// Gets a set of tetrahedrons that represent the volume of the grid. The method used
        /// creates 5 tetrahedra per cube.
        /// </summary>
        public IEnumerable<Tetrahedron<int>> Volume
        {
            get
            {
                IVector csize = this._Size - new IVector(1, 1, 1);
                for (int z = 0; z < csize.Z; z++)
                {
                    for (int y = 0; y < csize.Y; y++)
                    {
                        for (int x = 0; x < csize.X; x++)
                        {
                            int start = this.Index(new IVector(x, y, z));
                            int[] cubepoints = new int[8];
                            for (int t = 0; t < cubepoints.Length; t++)
                            {
                                cubepoints[t] = start;
                            }
                            for (int t = 0; t < 4; t++)
                            {
                                cubepoints[4 + t] += this._Size.Y * this._Size.X;
                                cubepoints[2 + ((t / 2) * 2) + t] += this._Size.X;
                                cubepoints[1 + (t * 2)] += 1;
                            }
                            foreach (Tetrahedron<int> tetra in Tetrahedron.Tesselate(cubepoints, (x + y + z) % 2 == 0))
                            {
                                yield return tetra;
                            }
                        }
                    }
                }

            }
        }

        private Lattice _Source;
        private IVector _Size;
    }
}