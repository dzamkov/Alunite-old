using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite.Fast
{
    /// <summary>
    /// Matter created by the composition of a untransformed and a transformed matter.
    /// </summary>
    public class BinaryMatter : Matter
    {
        public BinaryMatter(Matter A, Matter B, Transform AToB)
        {
            this._A = A;
            this._B = B;
            this._AToB = AToB;
        }

        /// <summary>
        /// Gets the untransformed part of this matter.
        /// </summary>
        public Matter A
        {
            get
            {
                return this._A;
            }
        }

        /// <summary>
        /// Gets the source of the transformed part of this matter.
        /// </summary>
        public Matter B
        {
            get
            {
                return this._B;
            }
        }

        /// <summary>
        /// Gets the transformed part of this matter with the transform applied.
        /// </summary>
        public TransformedMatter BFull
        {
            get
            {
                return new TransformedMatter(this._B, this._AToB);
            }
        }

        /// <summary>
        /// Gets the transform from A's (and this matter's) coordinate space to B's coordinate space.
        /// </summary>
        public Transform AToB
        {
            get
            {
                return this._AToB;
            }
        }

        public override void GetMassSummary(Physics Physics, out double Mass, out Vector CenterOfMass, out double Extent)
        {
            double amass, bmass;
            Vector acen, bcen;
            double aext, bext;
            this._A.GetMassSummary(Physics, out amass, out acen, out aext);
            this._B.GetMassSummary(Physics, out bmass, out bcen, out bext);
            bcen = this._AToB.ApplyToOffset(bcen);

            Mass = amass + bmass;
            CenterOfMass = acen * (amass / Mass) + bcen * (bmass / Mass);

            double alen = (acen - CenterOfMass).Length + aext;
            double blen = (bcen - CenterOfMass).Length + bext;
            Extent = Math.Max(alen, blen);
        }

        public override Matter Update(Physics Physics, Matter Environment, double Time)
        {
            Matter na = this.A.Update(Physics, Physics.Combine(this.B.Apply(Physics, this.AToB), Environment), Time);
            Matter nb = this.B.Update(Physics, Physics.Combine(this.A, Environment).Apply(Physics, this.AToB.Inverse), Time);
            return Physics.Combine(na, nb.Apply(Physics, this.AToB.Update(Time)));
        }

        public override void OutputUsed(HashSet<Matter> Elements)
        {
            Elements.Add(this);
            this._A.OutputUsed(Elements);
            this._B.OutputUsed(Elements);
        }

        public override void OutputParticles(Transform Transform, List<Particle<Substance>> Particles)
        {
            this._A.OutputParticles(Transform, Particles);
            this._B.OutputParticles(this._AToB.Apply(Transform), Particles);
        }

        public override Vector GetGravity(Physics Physics, Vector Position, double Mass, double RecurseThreshold)
        {
            Vector agrav = this.A.GetGravity(Physics, Position, Mass, RecurseThreshold);
            Vector bgrav = this.AToB.ApplyToDirection(this.B.GetGravity(Physics, this.AToB.Inverse.ApplyToOffset(Position), Mass, RecurseThreshold));
            return agrav + bgrav;
        }

        private Matter _A;
        private Matter _B;
        private Transform _AToB;
    }
}