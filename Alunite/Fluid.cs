using System;
using System.Collections.Generic;
using System.Linq;

using OpenTKGUI;

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

        private class _Substance : IVisualSubstance
        {
            public ISubstance Update(Matter Environment, double Time, ref Vector Position, ref Vector Velocity, ref Quaternion Orientation, ref double Mass)
            {
                // Loop through particles in the environment to find forces
                Vector force = new Vector(0.0, 0.0, 0.0);
                foreach (Particle p in Environment.Particles)
                {
                    Vector to = p.Position - Position;
                    double dis = to.Length;

                    // Gravity
                    force += to * (Particle.G * (p.Mass + Mass) / (dis * dis * dis));
                }

                Velocity += force * Time;
                Position += Velocity * Time;
                return this;
            }

            public Color Color
            {
                get
                {
                    return Color.RGB(0.0, 0.5, 1.0);
                }
            }
        }
    }
}