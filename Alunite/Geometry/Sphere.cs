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

        private double _Radius;
    }
}