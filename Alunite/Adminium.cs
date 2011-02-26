using System;
using System.Collections.Generic;
using System.Linq;

using OpenTKGUI;

namespace Alunite
{
    /// <summary>
    /// A simple substances that will not change velocity in response to a force, in effect making it static.
    /// </summary>
    public class Adminium : IVisualSubstance
    {
        public Color Color
        {
            get
            {
                return Color.RGB(0.5, 0.5, 0.5);
            }
        }

        public ISubstance Update(Matter Environment, double Time, ref Vector Position, ref Vector Velocity, ref Quaternion Orientation, ref double Mass)
        {
            Position += Velocity * Time;
            return this;
        }
    }
}