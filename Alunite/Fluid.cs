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
                Vector force = Environment.GetGravityForce(Position, Mass);

                double h = 0.1;
                foreach (Particle p in Environment.GetParticles(Position, h))
                {
                    Vector away = Position - p.Position;
                    double dis = away.Length;
                    force += away * (0.1 / (dis * dis * dis));
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