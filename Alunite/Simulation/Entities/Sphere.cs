using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A simple sphere-shaped physical entity.
    /// </summary>
    public class SphereEntity : Entity
    {
        public SphereEntity(double Mass, double Radius)
        {
            this._Mass = Mass;
            this._Radius = Radius;
        }

        /// <summary>
        /// Gets the mass of this entity in kilograms.
        /// </summary>
        public double Mass
        {
            get
            {
                return this._Mass;
            }
        }

        /// <summary>
        /// Gets the radius of this entity in meters.
        /// </summary>
        public double Radius
        {
            get
            {
                return this._Radius;
            }
        }

        private double _Mass;
        private double _Radius;
    }
}