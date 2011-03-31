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
            Signal<Vector> lookpos;
            Signal<Vector> looktar;
            Signal<Vector> lookup;
            {
                var path = CubicSignal.BuildVectorPath();
                path.Jump(new Vector(-5.0, 0.0, 0.0));
                path.Add(1.0, new Vector(0.0, -5.0, 1.0));
                path.Add(1.0, new Vector(5.0, 0.0, 2.0));
                path.Add(1.0, new Vector(0.0, 5.0, 3.0));
                lookpos = path.Finish();

                looktar = Signal.Constant(Vector.Origin);
                lookup = Signal.Constant(new Vector(0.0, 0.0, 1.0));
            }

            CameraEntity camera = Entity.Camera();
            MoverEntity mover = Entity.Mover(camera);

            EntityBuilder builder = Entity.Builder();
            builder.Add(mover);
            builder.Attach(mover.Input, Signal.LookAt(lookpos, looktar, lookup));
            builder.Add(Entity.Brush(Substance.Iron, Shape.Sphere(1.0)));
            Entity world = builder.Finish();

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.Control = new Visualizer(Span.Create(world).Read(camera.Output));
            hw.Run(60.0);
        }
    }
}