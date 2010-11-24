using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A sequence of instructions to be execute on the current graphics context.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Performs the instructions described by the renderable.
        /// </summary>
        void Render();
    }
}