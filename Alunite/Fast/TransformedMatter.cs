using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite.Fast
{
    /// <summary>
    /// Matter created by transforming other "Source" matter.
    /// </summary>
    public class TransformedMatter : Matter
    {
        public TransformedMatter(Matter Source, Transform Transform)
        {
            this._Source = Source;
            this._Transform = Transform;
        }

        /// <summary>
        /// Gets the source matter for this transformed matter.
        /// </summary>
        public Matter Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the transform applied by this transformed matter.
        /// </summary>
        public Transform Transform
        {
            get
            {
                return this._Transform;
            }
        }

        public override Matter Apply(Physics Physics, Transform Transform)
        {
            return new TransformedMatter(this.Source, this.Transform.Apply(Transform));
        }

        public override Matter Update(Physics Physics, Matter Environment, double Time)
        {
            return Physics.Apply(this.Source.Update(Physics, Physics.Apply(Environment, this.Transform.Inverse), Time), this.Transform.Update(Time));
        }

        public override void OutputUsed(HashSet<Matter> Elements)
        {
            Elements.Add(this);
            this._Source.OutputUsed(Elements);
        }

        public override void OutputParticles(Transform Transform, List<Particle<Substance>> Particles)
        {
            this._Source.OutputParticles(this._Transform.Apply(Transform), Particles);
        }

        public override void GetMassSummary(Physics Physics, out double Mass, out Vector CenterOfMass, out double Extent)
        {
            this._Source.GetMassSummary(Physics, out Mass, out CenterOfMass, out Extent);
            CenterOfMass = this._Transform.ApplyToOffset(CenterOfMass);
        }

        public override Vector GetGravity(Physics Physics, Vector Position, double Mass, double RecurseThreshold)
        {
            return this._Transform.ApplyToDirection(this._Source.GetGravity(Physics, this._Transform.Inverse.ApplyToOffset(Position), Mass, RecurseThreshold));
        }

        private Matter _Source;
        private Transform _Transform;
    }
}