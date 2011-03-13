using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A method of filling a shape with substances.
    /// </summary>
    public abstract class Material
    {
        /// <summary>
        /// Gets an aggregation of the matter in the brush created with this material and the specified shape.
        /// </summary>
        public abstract MassAggregate GetAggregate(Shape Shape);

        /// <summary>
        /// Gets a material with a single solid substance.
        /// </summary>
        public static SolidMaterial Solid(Substance Substance)
        {
            return new SolidMaterial(Substance);
        }
    }

    /// <summary>
    /// A simple material that fills a shape completely with a single substance.
    /// </summary>
    public class SolidMaterial : Material
    {
        public SolidMaterial(Substance Substance)
        {
            this._Substance = Substance;
        }

        /// <summary>
        /// Gets the substance used for this material.
        /// </summary>
        public Substance Substance
        {
            get
            {
                return this._Substance;
            }
        }

        public override MassAggregate GetAggregate(Shape Shape)
        {
            return new MassAggregate(
                this._Substance.Density * Shape.Volume,
                Shape.Centroid);
        }

        private Substance _Substance;
    }
}