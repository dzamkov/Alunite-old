using System;
using System.Collections.Generic;

using OpenTK;
using OpenTKGUI;

using Alunite.Fast;

namespace Alunite
{
    /// <summary>
    /// Program main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program main entry-point.
        /// </summary>
        public static void Main(string[] Args)
        {
            Curve<Vector> acceleration = Curve.Constant(new Vector(0.0, 0.0, -9.8));
            Curve<Vector> velocity = Curve.Integral(acceleration, new Vector(0.0, 0.0, 0.0));
            Curve<Vector> position = Curve.Integral(velocity, new Vector(0.0, 0.0, 0.0));
        }
    }
}