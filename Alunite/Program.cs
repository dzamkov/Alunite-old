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
            List<FastPhysicsMatter> elems = new List<FastPhysicsMatter>();
            Random r = new Random(2);
            List<Vector> pointset = new List<Vector>();
            for(int t = 0; t < 1000; t++)
            {
                Vector pos = new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble());
                elems.Add(fp.Create(new Particle<FastPhysicsSubstance>()
                {
                    Substance = FastPhysicsSubstance.Default,
                    Mass = 1.0,
                    Position = pos,
                    Velocity = new Vector(0.0, 0.0, 0.0),
                    Orientation = Quaternion.Identity,
                    Spin = AxisAngle.Identity
                }));
                pointset.Add(pos);
            }
            FastPhysicsMatter world = fp.Compose(elems);

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(world.Particles, pointset);
            hw.Run(60.0);
        }
    }
}