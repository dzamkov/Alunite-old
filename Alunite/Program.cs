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
            Primitive icosa = Primitive.Icosahedron;
            Diagram dia = icosa.CreateDiagram();
            dia.Display();
        }
    }
}