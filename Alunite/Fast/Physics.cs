using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite.Fast
{
    /// <summary>
    /// A physics system where interactions between matter are memozied allowing for
    /// faster simulation.
    /// </summary>
    public class Physics : IParticlePhysics<Matter, Substance>, IGravitationalPhysics<Matter>
    {
        public Physics(double G)
        {
            this._G = G;
        }

        public Physics()
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

        public Matter Create(Particle<Substance> Particle)
        {
            return new TransformedMatter(
                new ParticleMatter(Particle.Substance, Particle.Mass, Particle.Spin.Apply(Particle.Orientation.Conjugate)),
                new Transform(Particle.Position, Particle.Velocity, Particle.Orientation));
        }

        /// <summary>
        /// Creates a lattice of the specified object. The amount of items in the lattice is equal to (2 ^ (Log2Size * 3)).
        /// </summary>
        public Matter CreateLattice(Matter Object, int Log2Size, double Spacing)
        {
            // Get "major" spacing
            Spacing = Spacing * Math.Pow(2.0, Log2Size - 1);

            // Extract transform
            TransformedMatter trans = Object as TransformedMatter;
            if (trans != null)
            {
                Object = trans.Source;
                return this._CreateLattice(Object, Log2Size, Spacing).Apply(this, trans.Transform);
            }
            else
            {
                return this._CreateLattice(Object, Log2Size, Spacing);
            }
        }

        private Matter _CreateLattice(Matter Object, int Log2Size, double MajorSpacing)
        {
            // Create a lattice with major spacing
            if (Log2Size <= 0)
            {
                return Object;
            }
            if (Log2Size > 1)
            {
                Object = this._CreateLattice(Object, Log2Size - 1, MajorSpacing * 0.5);
            }

            BinaryMatter a = this.QuickCombine(Object, Object, new Transform(MajorSpacing, 0.0, 0.0));
            BinaryMatter b = this.QuickCombine(a, a, new Transform(0.0, MajorSpacing, 0.0));
            BinaryMatter c = this.QuickCombine(b, b, new Transform(0.0, 0.0, MajorSpacing));
            return c;
        }

        public Matter Apply(Matter Matter, Transform Transform)
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

        public Matter Update(Matter Matter, Matter Environment, double Time)
        {
            return Matter.Update(this, Environment, Time);
        }

        public Matter Compose(IEnumerable<Matter> Elements)
        {
            Matter cur = this.Null;
            foreach (Matter matter in Elements)
            {
                cur = this.Combine(cur, matter);
            }
            return cur;
        }

        /// <summary>
        /// Combines two pieces of matter in a similar manner to Compose.
        /// </summary>
        public Matter Combine(Matter A, Matter B)
        {
            if (A == null)
            {
                return B;
            }
            if (B == null)
            {
                return A;
            }

            TransformedMatter atrans = A as TransformedMatter;
            TransformedMatter btrans = B as TransformedMatter;

            Transform atob = Transform.Identity;
            if (btrans != null)
            {
                atob = btrans.Transform;
                B = btrans.Source;
            }

            if (atrans != null)
            {
                return new MemoizedBinaryMatter(atrans.Source, B, atob.Apply(atrans.Transform.Inverse)).Apply(this, atrans.Transform);
            }
            else
            {
                return new MemoizedBinaryMatter(A, B, atob);
            }
        }

        /// <summary>
        /// Quickly combines two nonnull pieces of untransformed matter.
        /// </summary>
        public BinaryMatter QuickCombine(Matter A, Matter B, Transform AToB)
        {
            return new MemoizedBinaryMatter(A, B, AToB);
        }

        public Matter Null
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

        public Vector GetGravity(Matter Environment, Vector Position, double Mass)
        {
            return Environment.GetGravity(this, Position, Mass, 0.0);
        }

        public double GetMass(Matter Matter)
        {
            double mass; Vector com; double extent;
            Matter.GetMassSummary(this, out mass, out com, out extent);
            return mass;
        }

        private double _G;
    }
}