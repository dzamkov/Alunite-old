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
            // Create a test world
            Random r = new Random();
            List<Matter> elems = new List<Matter>();
            BlobMatter worldblob = new BlobMatter(0.1);

            // Water
            for (int t = 0; t < 500; t++)
            {
                worldblob.Add(new Particle(
                    new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble() * 0.5),
                    0.01, Fluid.GetSubstance()));
            }

            // Adminium walls
            for (double x = 0.0; x <= 1.0; x += 0.04)
            {
                for (double y = 0.0; y <= 1.0; y += 0.04)
                {
                    worldblob.Add(new Particle(new Vector(x, y, 0.0), 0.01, new Adminium()));
                    worldblob.Add(new Particle(new Vector(x, 0.0, y), 0.01, new Adminium()));
                    worldblob.Add(new Particle(new Vector(0.0, x, y), 0.01, new Adminium()));
                    worldblob.Add(new Particle(new Vector(x, 1.0, y), 0.01, new Adminium()));
                    worldblob.Add(new Particle(new Vector(1.0, x, y), 0.01, new Adminium()));
                }
            }

            // "Earth"
            elems.Add(worldblob);
            elems.Add(new Particle(
                new Vector(0.0, 0.0, -6.3675e6),
                5.9721e24, new Adminium()).Matter);

            Matter world = CompositeMatter.Create(elems);

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(world);
            hw.Run(60.0);
        }
    }
}