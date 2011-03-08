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
            Transform testa = new Transform(
                new Vector(1.0, 0.5, 0.3),
                new Vector(1.0, 0.5, 0.3),
                Quaternion.AngleBetween(Vector.Normalize(new Vector(0.9, 0.8, 0.7)), new Vector(0.0, 0.0, 1.0)));
            testa = testa.ApplyTo(testa.Inverse);


            // Set up a world
            Physics fp = new Physics();
            Matter obj = fp.CreateLattice(fp.Create(new Particle<Substance>()
            {
                Substance = Substance.Default,
                Mass = 1.0,
                Position = new Vector(0.0, 0.0, 0.0),
                Orientation = Quaternion.Identity,
                Spin = AxisAngle.Identity
            }), 3, 0.1);

            Matter earth = fp.Create(new Particle<Substance>()
            {
                Substance = Substance.Default,
                Mass = 5.9742e24,
                Position = new Vector(0.0, 0.0, -6.3675e6),
                Orientation = Quaternion.Identity,
                Spin = AxisAngle.Identity
            });

            Matter world = obj;

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(fp, world);
            hw.Run(60.0);
        }
    }
}