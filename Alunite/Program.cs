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
            CameraEntity camsensor = Entity.Camera();
            Entity cambody = Entity.Sphere(0.1, 0.1).Apply(new Transform(-0.12, 0.0, 0.0));
            Entity cam = camsensor.Embody(cambody);
            Entity obj = Entity.Sphere(0.1, 2.1).Apply(new Transform(5.0, 0.0, 0.0));
            CompoundEntity world = Entity.Compound();
            OutTerminal<View> camout = world.Add(cam).Lookup(camsensor.Output);
            world.Add(obj);

            Simulation sim = Simulation.Create(world);

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.Control = new Visualizer(sim.Read(camout));
            hw.Run(60.0);
        }
    }
}