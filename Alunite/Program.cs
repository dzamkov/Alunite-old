using System;
using System.Collections.Generic;

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
            Transform a = new Transform(new Vector(0.7, 0.4, 1.0), new Vector(0.8, 0.1, 1.0), new Quaternion(new Vector(0.0, 1.0, 0.0), Math.PI / 3.0));
            Transform b = new Transform(new Vector(0.3, 1.0, 0.4), new Vector(0.3, 0.2, 1.0), new Quaternion(new Vector(0.0, 0.0, 1.0), Math.PI / 7.0));
            a = a.Apply(b).Apply(b.Inverse.Apply(a.Inverse));
        }
    }
}