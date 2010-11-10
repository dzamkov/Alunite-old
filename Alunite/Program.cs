using System;
using System.Collections.Generic;

namespace Alunite
{
    public static class Program
    {

        /// <summary>
        /// Program main entry point.
        /// </summary>
        public static void Main(string[] Args)
        {
            Diagram d = new Diagram();
            int a = d.AddVertex(new Vector(0.0, 0.0, 0.8));
            int b = d.AddVertex(new Vector(1.0, 0.0, 0.1));
            int c = d.AddVertex(new Vector(0.0, 1.0, 0.1));
            d.SetBorderedTriangle(new Triangle<int>(a, b, c), Color.RGBA(1.0, 1.0, 1.0, 1.0), Color.RGB(1.0, 0.0, 0.0), 3.0);
            d.Display();
        }
    }
}