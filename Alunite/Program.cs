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
            Planet planet = new Planet();
            Diagram dia = planet.CreateDiagram();
            dia.Display();
        }
    }
}