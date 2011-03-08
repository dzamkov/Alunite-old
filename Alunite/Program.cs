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

            Curve<Scalar> curve = new Curve<Scalar>(new Scalar[]
            {
                1.0, -1.0, 1.0, -1.0, 1.0, -1.0, 1.0, 0.6, 0.2, 0.3, 0.4, 0.5
            });
            double val = curve[1.0];
        }
    }
}