using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// A physics system where interactions between matter are memozied allowing for
    /// faster simulation.
    /// </summary>
    public class FastPhysics : IParticlePhysics<FastPhysicsMatter, FastPhysicsSubstance>, IGravitationalPhysics<FastPhysicsMatter>
    {
        public FastPhysics(double G)
        {
            this._G = G;
        }

        public FastPhysics()
            : this(6.67428e-11)
        {

        }

        /// <summary>
        /// Gets the gravitational constant for this physical system in newton meters / kilogram ^ 2.
        /// </summary>
        public double G
        {
            get
            {
                return this._G;
            }
        }

        public FastPhysicsMatter Create(Particle<FastPhysicsSubstance> Particle)
        {
            return FastPhysicsMatter.Particle(this, Particle);
        }

        public FastPhysicsMatter Transform(FastPhysicsMatter Matter, Transform Transform)
        {
            if (Matter != null)
            {
                return Matter.Apply(this, Transform);
            }
            else
            {
                return null;
            }
        }

        public FastPhysicsMatter Update(FastPhysicsMatter Matter, FastPhysicsMatter Environment, double Time)
        {
            return Matter.Update(this, Environment, Time);
        }

        public FastPhysicsMatter Compose(IEnumerable<FastPhysicsMatter> Elements)
        {
            FastPhysicsMatter cur = this.Null;
            foreach (FastPhysicsMatter matter in Elements)
            {
                cur = this.Combine(cur, matter);
            }
            return cur;
        }

        /// <summary>
        /// Combines two pieces of matter in a similar manner to Compose.
        /// </summary>
        public FastPhysicsMatter Combine(FastPhysicsMatter A, FastPhysicsMatter B)
        {
            return FastPhysicsMatter.Combine(this, A, B);
        }

        public FastPhysicsMatter Null
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the gravity an object will feel towards a planet (and vice versa) when the object is at the given
        /// offset in meters.
        /// </summary>
        public Vector GetGravity(double PlanetMass, Vector Offset, double Mass)
        {
            double sqrlen = Offset.SquareLength;
            return Offset * (-this._G * (PlanetMass + Mass) / (sqrlen * Math.Sqrt(sqrlen)));
        }

        /// <summary>
        /// Gets the strength in newtons of the gravity force between the two objects. Note that negatives indicate attraction and positives repulsion.
        /// </summary>
        public double GetGravityStrength(double MassA, double MassB, double Distance)
        {
            return -this._G * (MassA + MassB) / (Distance * Distance);
        }

        public Vector GetGravity(FastPhysicsMatter Environment, Vector Position, double Mass)
        {
            return Environment.GetGravity(this, Position, Mass, 0.0);
        }

        public double GetMass(FastPhysicsMatter Matter)
        {
            double mass; Vector com; double extent;
            Matter.GetMass(out mass, out com, out extent);
            return mass;
        }

        private double _G;
    }
}