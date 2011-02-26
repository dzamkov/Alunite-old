using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Matter composed of efficently organized particles that can only interact in a limited range. Note that it
    /// is assumed that the "blob" is too light to create a gravitational force.
    /// </summary>
    public class BlobMatter : CompositeMatter
    {
        public BlobMatter(double GridSize)
        {
            this._GridSize = GridSize;
            this._Grid = new Dictionary<_GridRef, List<Particle>>(_GridRef.EqualityComparer.Singleton);
        }

        private BlobMatter(double GridSize, Dictionary<_GridRef, List<Particle>> Grid)
        {
            this._GridSize = GridSize;
            this._Grid = Grid;
        }

        /// <summary>
        /// Gets size of units in the grid used to organize particles. This must be above the interaction
        /// distance of particles in the grid.
        /// </summary>
        public double GridSize
        {
            get
            {
                return this._GridSize;
            }
        }

        public override IEnumerable<Particle> Particles
        {
            get
            {
                return
                    from pl in this._Grid.Values
                    from p in pl
                    select p;
            }
        }

        public override IEnumerable<Matter> Elements
        {
            get
            {
                return
                    from p in this.Particles
                    select p.Matter;
            }
        }

        public override Vector GetGravityForce(Vector Position, double Mass)
        {
            return new Vector(0.0, 0.0, 0.0);
        }

        public override Matter Update(Matter Environment, double Time)
        {
            var ngrid = new Dictionary<_GridRef, List<Particle>>(_GridRef.EqualityComparer.Singleton);

            foreach (var kvp in this._Grid)
            {
                List<Particle> inunit = kvp.Value;

                // Get surronding units the particles in this unit can interact with.
                List<List<Particle>> surrondunit = new List<List<Particle>>();
                _GridRef gref = kvp.Key;
                for (int x = gref.X - 1; x <= gref.X + 1; x++)
                {
                    for (int y = gref.Y - 1; y <= gref.Y + 1; y++)
                    {
                        for (int z = gref.Z - 1; z <= gref.Z + 1; z++)
                        {
                            _GridRef sref = new _GridRef(x, y, z);
                            if (sref.X != gref.X || sref.Y != gref.Y || sref.Z != gref.Z)
                            {
                                List<Particle> sunit;
                                if (this._Grid.TryGetValue(sref, out sunit))
                                {
                                    surrondunit.Add(sunit);
                                }
                            }
                        }
                    }
                }

                // Update particles
                for (int t = 0; t < inunit.Count; t++)
                {
                    Particle p = inunit[t];
                    _GridEnvironment e = new _GridEnvironment()
                    {
                        InUnit = inunit,
                        CurIndex = t,
                        SurrondUnit = surrondunit
                    };
                    p.Substance = p.Substance.Update(
                        BinaryMatter.Create(Environment, e),
                        Time,
                        ref p.Position,
                        ref p.Velocity,
                        ref p.Orientation,
                        ref p.Mass);
                    _Add(ngrid, this._GridSize, p);
                }
            }

            return new BlobMatter(this._GridSize, ngrid);
        }

        /// <summary>
        /// Gets the grid reference for the specified position.
        /// </summary>
        private static _GridRef _ForPos(Vector Position, double GridSize)
        {
            return new _GridRef(
                (int)(Position.X / GridSize),
                (int)(Position.Y / GridSize),
                (int)(Position.Z / GridSize));
        }

        /// <summary>
        /// Adds a particle to a grid.
        /// </summary>
        private static void _Add(Dictionary<_GridRef, List<Particle>> Grid, double GridSize, Particle Particle)
        {
            _GridRef gref = _ForPos(Particle.Position, GridSize);
            List<Particle> unit;
            if (!Grid.TryGetValue(gref, out unit))
            {
                unit = Grid[gref] = new List<Particle>();
            }
            unit.Add(Particle);
        }

        /// <summary>
        /// Adds a particle to this blob.
        /// </summary>
        public void Add(Particle Particle)
        {
            _Add(this._Grid, this._GridSize, Particle);
        }

        /// <summary>
        /// An environment given to a particle on the grid.
        /// </summary>
        private class _GridEnvironment : Matter
        {
            public override IEnumerable<Particle> Particles
            {
                get
                {
                    for (int t = 0; t < this.InUnit.Count; t++)
                    {
                        if (t != this.CurIndex)
                        {
                            yield return this.InUnit[t];
                        }
                    }
                    foreach (List<Particle> unit in this.SurrondUnit)
                    {
                        foreach (Particle p in unit)
                        {
                            yield return p;
                        }
                    }
                }
            }

            public override Vector GetGravityForce(Vector Position, double Mass)
            {
                return new Vector(0.0, 0.0, 0.0);
            }

            public override Matter Update(Matter Environment, double Time)
            {
                throw new NotImplementedException();
            }

            public int CurIndex;
            public List<Particle> InUnit;
            public List<List<Particle>> SurrondUnit;
        }

        /// <summary>
        /// A reference to a unit on the grid.
        /// </summary>
        private struct _GridRef
        {
            public _GridRef(int X, int Y, int Z)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
            }

            public int X;
            public int Y;
            public int Z;

            public class EqualityComparer : IEqualityComparer<_GridRef>
            {
                public static readonly EqualityComparer Singleton = new EqualityComparer();

                public bool Equals(_GridRef x, _GridRef y)
                {
                    return x.X == y.X && x.Y == y.Y && x.Z == y.Z;
                }

                public int GetHashCode(_GridRef obj)
                {
                    return obj.X ^ (obj.Y + 0x1337BED5) ^ (obj.Z + 0x12384923) ^ (obj.Y << 3) ^ (obj.Y >> 3) ^ (obj.Z << 7) ^ (obj.Z >> 7);
                } 
            }
        }

        private Dictionary<_GridRef, List<Particle>> _Grid;
        private double _GridSize;
    }
}