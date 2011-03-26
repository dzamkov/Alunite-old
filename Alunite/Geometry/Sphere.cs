using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// A mask for a spherical shape centered on the origin.
    /// </summary>
    public class Sphere : Mask
    {
        public Sphere(double Radius)
        {
            this._Radius = Radius;
        }

        public override bool this[Vector Offset]
        {
            get
            {
                return Offset.SquareLength <= this._Radius * this._Radius;
            }
        }

        public override Vector Centriod
        {
            get
            {
                return Vector.Origin;
            }
        }

        public override double Volume
        {
            get
            {
                return (4.0 / 3.0) * Math.PI * this._Radius * this._Radius * this._Radius;
            }
        }

        public override Surface<Void> Surface
        {
            get 
            {
                return new SphereSurface(this._Radius);
            }
        }

        private double _Radius;
    }

    /// <summary>
    /// The surface of a sphere of a certain radius.
    /// </summary>
    public class SphereSurface : Foil
    {
        public SphereSurface(double Radius)
        {
            this._Radius = Radius;
        }

        public override IEnumerable<SurfaceHit<Void>> Trace(Segment<Vector> Segment)
        {
            Vector m = Segment.B - Segment.A;
            double n = m.Length;
            Vector l = m * (1.0 / n);
            Vector c = -Segment.A;
            double cl = Vector.Dot(c, l);
            double r = cl * cl - Vector.Dot(c, c) + this._Radius * this._Radius;
            if (r >= 0.0)
            {
                double d = cl - Math.Sqrt(r);
                if (d >= 0.0 && d < n)
                {
                    Vector offset = Segment.A + l * d;
                    return new SurfaceHit<Void>[]
                    {
                        new SurfaceHit<Void>(Void.Value, d / n, offset, Vector.Normalize(offset))
                    };
                }
            }
            return Enumerable.Empty<SurfaceHit<Void>>();
        }

        public override Mesh<Void> ApproximateMesh(int Triangles)
        {
            List<Vector> verts = new List<Vector>(12);
            List<Triangle<int>> tris = new List<Triangle<int>>(20);

            // Set vertices
            double phi = (1.0 + Math.Sqrt(5)) / 2.0; // Golden ratio FTW!
            double ea = 1.0 / Math.Sqrt(phi * phi + 1);
            double eb = phi * ea;
            ea *= this._Radius;
            eb *= this._Radius;
            verts.Add(new Vector(-ea, -eb, 0));
            verts.Add(new Vector(0, -ea, -eb));
            verts.Add(new Vector(-eb, 0, -ea));
            verts.Add(new Vector(-ea, eb, 0));
            verts.Add(new Vector(0, -ea, eb));
            verts.Add(new Vector(eb, 0, -ea));
            verts.Add(new Vector(ea, -eb, 0));
            verts.Add(new Vector(0, ea, -eb));
            verts.Add(new Vector(-eb, 0, ea));
            verts.Add(new Vector(ea, eb, 0));
            verts.Add(new Vector(0, ea, eb));
            verts.Add(new Vector(eb, 0, ea));

            // Set edge pairs
            tris.Add(new Triangle<int>(0, 8, 2));
            tris.Add(new Triangle<int>(3, 2, 8));
            tris.Add(new Triangle<int>(1, 6, 0));
            tris.Add(new Triangle<int>(4, 0, 6));
            tris.Add(new Triangle<int>(2, 7, 1));
            tris.Add(new Triangle<int>(5, 1, 7));
            tris.Add(new Triangle<int>(6, 5, 11));
            tris.Add(new Triangle<int>(9, 11, 5));
            tris.Add(new Triangle<int>(7, 3, 9));
            tris.Add(new Triangle<int>(10, 9, 3));
            tris.Add(new Triangle<int>(8, 4, 10));
            tris.Add(new Triangle<int>(11, 10, 4));

            // Set inner triangles
            tris.Add(new Triangle<int>(0, 4, 8));
            tris.Add(new Triangle<int>(8, 10, 3));
            tris.Add(new Triangle<int>(1, 5, 6));
            tris.Add(new Triangle<int>(2, 1, 0));
            tris.Add(new Triangle<int>(10, 11, 9));
            tris.Add(new Triangle<int>(2, 3, 7));
            tris.Add(new Triangle<int>(5, 7, 9));
            tris.Add(new Triangle<int>(6, 11, 4));

            // Refine
            while (tris.Count < Triangles)
            {
                Dictionary<Segment<int>, int> midpoints = new Dictionary<Segment<int>, int>(Segment<int>.GetDirectedComparer(EqualityComparer<int>.Default));
                List<Triangle<int>> ntris = new List<Triangle<int>>(tris.Count * 4);
                foreach (Triangle<int> tri in tris)
                {
                    int[] centralverts = new int[3];
                    int i = 0;
                    foreach (Segment<int> triseg in tri.Segments)
                    {
                        int vert;
                        if (midpoints.TryGetValue(triseg, out vert))
                        {
                            midpoints.Remove(triseg);
                        }
                        else
                        {
                            vert = verts.Count;
                            verts.Add(Vector.Normalize(Segment.Centroid(new Segment<Vector>(
                                verts[triseg.A],
                                verts[triseg.B]))) * this._Radius);
                            midpoints.Add(triseg.Flip, vert);
                        }
                        centralverts[i] = vert;
                        i++;
                    }

                    Triangle<int> central = new Triangle<int>(centralverts[0], centralverts[1], centralverts[2]);

                    ntris.Add(central);
                    ntris.Add(new Triangle<int>(tri.A, central.A, central.C));
                    ntris.Add(new Triangle<int>(tri.B, central.B, central.A));
                    ntris.Add(new Triangle<int>(tri.C, central.C, central.B));
                }

                tris = ntris;
            }

            return new ListMesh(verts, tris);
        }

        private double _Radius;
    }
}