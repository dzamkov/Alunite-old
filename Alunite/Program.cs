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
            Quaternion uprot = new Quaternion(new Vector(0.0, 0.0, 1.0), Math.PI / 16.0);
            Vector test = new Vector(0.9, 0.5, 0.5);
            test = uprot.Rotate(test);

        }
    }
}