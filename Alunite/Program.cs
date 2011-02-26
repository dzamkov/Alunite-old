using System;
using System.Collections.Generic;

using OpenTK;
using OpenTKGUI;

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
            double gforce = 5.9e24 / Math.Pow(6.367e6, 2.0) * Particle.G;

            // Create a test world
            Random r = new Random();
            List<Matter> elems = new List<Matter>();
            for (int t = 0; t < 100; t++)
            {
                elems.Add(new Particle(
                    new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble()),
                    new Vector(0.0, 0.0, 0.0),
                    Quaternion.Identity,
                    0.01, null).Matter);
            }
            Matter world = CompositeMatter.Create(elems);

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(world);
            hw.Run(60.0);
        }
    }
}