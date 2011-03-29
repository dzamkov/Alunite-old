using System;
using System.Collections.Generic;
using System.Linq;

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

            EntityBuilder builder = Entity.Builder();
            CameraEntity camsensor = Entity.Camera();
            builder.Add(camsensor);
            builder.Embody(Entity.Brush(Substance.Iron, Shape.Sphere(0.1)));
            builder.Apply(new Transform(-0.12, 0.0, 0.0));
            builder.Add(Entity.Brush(Substance.Iron, Shape.Sphere(1.0)).Apply(new Transform(5.0, 0.0, 0.0)));
            Entity world = builder.Finish();

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.Control = new Visualizer(null);
            hw.Run(60.0);
        }
    }
}