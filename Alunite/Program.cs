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
            Quaternion tq = Quaternion.AxisAngle(Vector.Normalize(new Vector(0.5, 0.7, 0.8)), 0.6);
            OrthogonalMatrix om = tq;

            Vector test = new Vector(0.0, 1.0, 2.0);
            Vector ra = tq.Rotate(test);
            Vector rb = om.Apply(test);
            Vector rc = Quaternion.FromMatrix(om).Rotate(test);

            EntityBuilder builder = Entity.Builder();
            CameraEntity camsensor = Entity.Camera();
            builder.Add(camsensor);
            builder.Embody(Entity.Brush(Substance.Iron, Shape.Sphere(0.1)));
            builder.Apply(new Transform(-0.12, 0.0, 0.0));
            builder.Add(Entity.Brush(Substance.Iron, Shape.Sphere(1.0)).Apply(new Transform(5.0, 0.0, 0.0)));
            Entity world = builder.Finish();

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.Control = new Visualizer(Span.Natural(world).Read(camsensor.Output));
            hw.Run(60.0);
        }
    }
}