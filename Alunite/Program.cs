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
            Transform testa = new Transform(
                new Vector(1.0, 0.5, 0.3),
                new Vector(1.0, 0.5, 0.3),
                Quaternion.AngleBetween(Vector.Normalize(new Vector(0.9, 0.8, 0.7)), new Vector(0.0, 0.0, 1.0)));
            testa = testa.ApplyTo(testa.Inverse);


            // Set up a world
            FastPhysics fp = new FastPhysics();
            FastPhysicsMatter world = FastPhysicsMatter.CreateLattice(fp, 1, fp.Create(new Particle<FastPhysicsSubstance>()
                        {
                            Substance = FastPhysicsSubstance.Default,
                            Mass = 1.0,
                            Position = new Vector(0.0, 0.0, 0.0),
                            Velocity = new Vector(0.0, 0.0, 0.0),
                            Orientation = Quaternion.Identity,
                            Spin = AxisAngle.Identity
                        }), 0.1);

            world = world.Update(fp, fp.Null, 0.1);

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(world.Particles);
            hw.Run(60.0);
        }
    }
}