using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A purely physical entity defined by a shape mapping substances to offsets in space.
    /// </summary>
    public class Brush : Entity
    {
        public Brush(Shape<Substance> Shape)
        {
            this._Shape = Shape;
        }

        /// <summary>
        /// Gets the shape of this brush.
        /// </summary>
        public Shape<Substance> Shape
        {
            get
            {
                return this._Shape;
            }
        }

        public override MassAggregate Aggregate
        {
            get
            {
                return GetAggregate(this._Shape);
            }
        }

        /// <summary>
        /// Gets the mass aggregate for the given shape.
        /// </summary>
        public static MassAggregate GetAggregate(Shape<Substance> Shape)
        {
            MappedShape<bool, Substance> ms = Shape as MappedShape<bool, Substance>;
            if (ms != null)
            {
                Substance uniform = ms.Map(true);
                Substance background = ms.Map(false);

                if (background != Substance.Vacuum)
                {
                    return new MassAggregate(double.PositiveInfinity, new Vector(double.NaN, double.NaN, double.NaN));
                }

                Mask m = ms.Source as Mask;
                if (m != null)
                {
                    return new MassAggregate(uniform.Density * m.Volume, m.Centriod);
                }
            }

            throw new NotImplementedException();
        }

        public override bool Phantom
        {
            get
            {
                return false;
            }
        }

        private Shape<Substance> _Shape;
    }
}