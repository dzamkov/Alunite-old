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
            Entity cambody = Entity.Brush(Substance.Iron, Shape.Sphere(0.1)).Apply(new Transform(-0.12, 0.0, 0.0));
            Entity cam = camsensor.Embody(cambody);
            Entity obj = Entity.Brush(Substance.Iron, Shape.Sphere(1.0)).Apply(new Transform(5.0, 0.0, 0.0));
            Entity world = Entity.Combine(cam, obj);

            Span worldspan = Span.Create(double.PositiveInfinity, world);

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.Control = new Visualizer(worldspan.Read(camsensor.Output));
            hw.Run(60.0);
        }
    }
}