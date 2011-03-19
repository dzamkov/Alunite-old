using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A purely physical entity defined by a material applied to a shape.
    /// </summary>
    public class Brush : Entity
    {
        public Brush(Material Material, Shape Shape)
        {
            this._Material = Material;
            this._Shape = Shape;
        }

        /// <summary>
        /// Gets the material for this brush.
        /// </summary>
        public Material Material
        {
            get
            {
                return this._Material;
            }
        }

        /// <summary>
        /// Gets the shape of this brush.
        /// </summary>
        public Shape Shape
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
                return this._Material.GetAggregate(this._Shape);
            }
        }

        public override bool Phantom
        {
            get
            {
                return false;
            }
        }

        public override Span CreateSpan(Span Environment, ControlInput Input)
        {
            return StaticSpan.CreateSimple(this);
        }

        private Material _Material;
        private Shape _Shape;
    }
}