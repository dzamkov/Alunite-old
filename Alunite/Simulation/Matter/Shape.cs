using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Defines a filled area in space.
    /// </summary>
    public abstract class Shape
    {
        /// <summary>
        /// Gets if this shape occupies the specified point.
        /// </summary>
        public abstract bool Occupies(Vector Point);

        /// <summary>
        /// Gets the volume of the shape.
        /// </summary>
        public abstract double Volume { get; }

        /// <summary>
        /// Gets the centroid, or center of volume, of this shape.
        /// </summary>
        public abstract Vector Centroid { get; }

        /// <summary>
        /// Creates a sphere with the given radius.
        /// </summary>
        public static SphereShape Sphere(double Radius)
        {
            return new SphereShape(Radius);
        }
    }

    /// <summary>
    /// The shape of a sphere with a certain radius centered on the origin.
    /// </summary>
    public class SphereShape : Shape
    {
        public SphereShape(double Radius)
        {
            this._Radius = Radius;
        }

        public override bool Occupies(Vector Point)
        {
            return Point.SquareLength < (this._Radius * this._Radius);
        }

        public override double Volume
        {
            get
            {
                return (4.0 / 3.0) * Math.PI * this._Radius * this._Radius * this._Radius;
            }
        }

        public override Vector Centroid
        {
            get
            {
                return new Vector(0.0, 0.0, 0.0);
            }
        }

        private double _Radius;
    }
}