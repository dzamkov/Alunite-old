using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Alunite
{
    /// <summary>
    /// Defines a mapping of values in a three-dimensional volume.
    /// </summary>
    public abstract class Shape<T> : Data<Shape<T>>
    {
        /// <summary>
        /// Gets the value of this shape at the given offset.
        /// </summary>
        public abstract T this[Vector Offset] { get; }

        /// <summary>
        /// Maps all the values in this shape using the given mapping function.
        /// </summary>
        public virtual Shape<F> Map<F>(Expression<Func<T, F>> Map)
        {
            return MappedShape<T, F>.Create(this, Map);
        }
    }

    /// <summary>
    /// A shape defined by a mapping of values from a source shape.
    /// </summary>
    public abstract class MappedShape<T, F> : Shape<F>
    {
        /// <summary>
        /// Gets the source shape.
        /// </summary>
        public abstract Shape<T> Source { get; }

        /// <summary>
        /// Gets the mapping for the given value.
        /// </summary>
        public abstract F Map(T Value);

        /// <summary>
        /// Gets an expression form of the mapping function used for this shape. Implementing this property is optional
        /// and should only be used for optimization purposes. Null can be returned in this case.
        /// </summary>
        public virtual Expression<Func<T, F>> Mapping
        {
            get
            {
                return null;
            }
        }

        public override F this[Vector Offset]
        {
            get
            {
                return this.Map(this.Source[Offset]);
            }
        }

        /// <summary>
        /// Creates a new mapped shape given an expression representing the mapping.
        /// </summary>
        public static MappedShape<T, F> Create(Shape<T> Source, Expression<Func<T, F>> Map)
        {
            return new _Mapping(Source, Map);
        }

        /// <summary>
        /// A concrete implementation of a mapping using delegates.
        /// </summary>
        private class _Mapping : MappedShape<T, F>
        {
            public _Mapping(Shape<T> Source, Expression<Func<T, F>> Map)
            {
                this._Source = Source;
                this._Map = Map.Compile();
                this._MapExp = Map;
            }

            public override Shape<T> Source
            {
                get
                {
                    return this._Source;
                }
            }

            public override F Map(T Value)
            {
                return this._Map(Value);
            }

            public override Expression<Func<T, F>> Mapping
            {
                get
                {
                    return this._MapExp;
                }
            }

            private Shape<T> _Source;
            private Func<T, F> _Map;
            private Expression<Func<T, F>> _MapExp;
        }
    }

    /// <summary>
    /// A shape that defines an occupied region.
    /// </summary>
    public abstract class Mask : Shape<bool>
    {
        /// <summary>
        /// Gets the centriod for the mask. This is the average of the offsets of all points occupied by the mask.
        /// </summary>
        public abstract Vector Centriod { get; }

        /// <summary>
        /// Gets the volume of the mask in cubic units.
        /// </summary>
        public abstract double Volume { get; }

        /// <summary>
        /// Gets the surface of this mask.
        /// </summary>
        public abstract Surface<Void> Surface { get; }
    }

    /// <summary>
    /// Contains functions and methods related to shapes.
    /// </summary>
    public static class Shape
    {
        /// <summary>
        /// Gets a mask for a sphere of the given radius.
        /// </summary>
        public static Sphere Sphere(double Radius)
        {
            return new Sphere(Radius);
        }
    }
}