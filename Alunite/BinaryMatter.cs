using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Matter composed of two elements.
    /// </summary>
    public class BinaryMatter : CompositeMatter
    {
        public BinaryMatter(Matter A, Matter B)
        {
            this._A = A;
            this._B = B;
        }

        /// <summary>
        /// Creates some binary matter.
        /// </summary>
        public static BinaryMatter Create(Matter A, Matter B)
        {
            return new BinaryMatter(A, B);
        }

        public override IEnumerable<Matter> Elements
        {
            get
            {
                yield return this._A;
                yield return this._B;
            }
        }

        public override Matter Apply(Transform Transform)
        {
            return BinaryMatter.Create(
                this._A.Apply(Transform),
                this._B.Apply(Transform));
        }

        public override Matter Update(Matter Environment, double Time)
        {
            Matter na = this._A.Update(BinaryMatter.Create(Environment, this._B), Time);
            Matter nb = this._B.Update(BinaryMatter.Create(Environment, this._A), Time);
            return BinaryMatter.Create(na, nb);
        }

        private Matter _A;
        private Matter _B;
    }
}