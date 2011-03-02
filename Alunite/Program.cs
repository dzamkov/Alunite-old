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
            // Set up a world
            FastPhysics fp = new FastPhysics();
            List<FastPhysicsMatter> elems = new List<FastPhysicsMatter>();
            Random r = new Random();
            for(int t = 0; t < 1000; t++)
            {
                elems.Add(fp.Create(new Particle<FastPhysicsSubstance>()
                {
                    Substance = FastPhysicsSubstance.Default,
                    Mass = 1.0,
                    Position = new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble()),
                    Velocity = new Vector(0.0, 0.0, 0.0),
                    Orientation = Quaternion.Identity,
                    Spin = Quaternion.Identity
                }));
            }
            FastPhysicsMatter world = fp.Compose(elems);

            for (int t = 0; t < 10; t++)
            {
                world = fp.Update(world, fp.Null, 0.1);
            }

            /*
            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(world);
            hw.Run(60.0);*/
        }
    }
}