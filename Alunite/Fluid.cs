using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Functions related to fluids.
    /// </summary>
    public static class Fluid
    {
        /// <summary>
        /// Gets a substance for a fluid.
        /// </summary>
        public static ISubstance GetSubstance()
        {
            return new _Substance();
        }

        private class _Substance : ISubstance
        {
            public ISubstance Update(Matter Environment, double Time, ref Vector Position, ref Vector Velocity, ref Quaternion Orientation, ref double Mass)
            {
                Velocity.Z -= 0.1 * Time;
                Position += Velocity * Time;
                return this;
            }
        }
    }
}