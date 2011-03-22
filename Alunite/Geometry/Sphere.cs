using System;
using System.Collections.Generic;

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
                throw new NotImplementedException(); 
            }
        }

        private double _Radius;
    }

}