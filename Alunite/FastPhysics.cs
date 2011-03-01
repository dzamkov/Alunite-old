using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// A physics system where interactions between matter are memozied allowing for
    /// faster simulation.
    /// </summary>
    public class FastPhysics : IParticlePhysics<FastPhysicsMatter, FastPhysicsSubstance>
    {
        public FastPhysicsMatter Create(Particle<FastPhysicsSubstance> Particle)
        {
            return FastPhysicsMatter.Particle(this, Particle);
        }

        public FastPhysicsMatter Transform(FastPhysicsMatter Matter, Transform Transform)
        {
            return Matter.Apply(this, Transform);
        }

        public FastPhysicsMatter Update(FastPhysicsMatter Matter, FastPhysicsMatter Environment, double Time)
        {
            throw new NotImplementedException();
        }

        public FastPhysicsMatter Compose(IEnumerable<FastPhysicsMatter> Matter)
        {
            throw new NotImplementedException();
        }

        public FastPhysicsMatter Null
        {
            get
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Matter in a fast physics system.
    /// </summary>
    public abstract class FastPhysicsMatter : IMatter
    {
        private FastPhysicsMatter()
        {

        }

        /// <summary>
        /// Applies a transform to this matter.
        /// </summary>
        public virtual FastPhysicsMatter Apply(FastPhysics Physics, Transform Transform)
        {
            return new _Transformed()
            {
                Source = this,
                Transform = Transform
            };
        }

        /// <summary>
        /// Creates matter for a particle.
        /// </summary>
        public static FastPhysicsMatter Particle(FastPhysics Physics, Particle<FastPhysicsSubstance> Particle)
        {
            return new _Transformed()
            {
                Transform = new Transform(Particle.Position, Particle.Velocity, Particle.Orientation),
                Source = new _Particle()
                {
                    Mass = Particle.Mass,
                    Spin = Particle.Spin,
                    Substance = Particle.Substance
                }
            };
        }

        /// <summary>
        /// Matter containing a single particle.
        /// </summary>
        internal class _Particle : FastPhysicsMatter
        {
            public ISubstance Substance;
            public double Mass;
            public Quaternion Spin;
        }

        /// <summary>
        /// Matter created by transforming some source matter.
        /// </summary>
        internal class _Transformed : FastPhysicsMatter
        {
            public override FastPhysicsMatter Apply(FastPhysics Physics, Transform Transform)
            {
                return new _Transformed()
                {
                    Source = this.Source,
                    Transform = Transform.ApplyTo(this.Transform)
                };
            }

            public FastPhysicsMatter Source;
            public Transform Transform;
        }

        /// <summary>
        /// Matter created by the combination of some untransformed matter and some
        /// transformed matter.
        /// </summary>
        internal class _Binary : FastPhysicsMatter
        {
            public FastPhysicsMatter A;
            public FastPhysicsMatter B;
            public Transform AToB;
        }
    }

    /// <summary>
    /// A substance in a fast physics system.
    /// </summary>
    public class FastPhysicsSubstance : IAutoSubstance<FastPhysics, FastPhysicsMatter, FastPhysicsSubstance>
    {
        private FastPhysicsSubstance()
        {

        }

        /// <summary>
        /// The default (and currently only) possible substance.
        /// </summary>
        public static readonly FastPhysicsSubstance Default = new FastPhysicsSubstance();

        public void Update(FastPhysics Physics, FastPhysicsMatter Environment, double Time, ref Particle<FastPhysicsSubstance> Particle)
        {
            Particle.Update(Time);
        }
    }
}