using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Defines a mapping of values on a three-dimensional surface. The surface is directed (has a front and back face) and
    /// can be open or closed.
    /// </summary>
    public abstract class Surface<T> : Data<Surface<T>>
    {
        /// <summary>
        /// Gets information about all front-face points hit by the given directed line segment.
        /// </summary>
        public abstract IEnumerable<SurfaceHit<T>> Trace(Vector Start, Vector End);
    }

    /// <summary>
    /// A surface with no data associated with points.
    /// </summary>
    public abstract class Foil : Surface<Void>
    {

    }

    /// <summary>
    /// Represents a front-face hit on a surface by a directed line segment.
    /// </summary>
    public struct SurfaceHit<T>
    {
        /// <summary>
        /// The value of the surface on the point that was hit.
        /// </summary>
        public T Value;

        /// <summary>
        /// The length, in relation to the line segment, where the hit is.
        /// </summary>
        public double Length;

        /// <summary>
        /// The offset of the hit from the surface.
        /// </summary>
        public Vector Offset;

        /// <summary>
        /// The normal of the hit, or a zero vector if the surface is discontinous at the hit point.
        /// </summary>
        public Vector Normal;
    }
}