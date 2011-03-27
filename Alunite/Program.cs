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
            CubicSignal<double, ScalarContinuum> a = CubicSignal<double, ScalarContinuum>.Linear(new ScalarContinuum(), 1.0, -1.0, 1.0);
            CubicSignal<double, ScalarContinuum> b = CubicSignal<double, ScalarContinuum>.Linear(new ScalarContinuum(), 1.0, -1.0, 1.0);
            a = a.Resample(10);
            b = b.Resample(10);
            CubicSignal<double, ScalarContinuum> c = CubicSignal<double, ScalarContinuum>.Sum(a, b, new ScalarContinuum());

            double error = 0.0;

            Random r = new Random();
            for (int i = 0; i < 100; i++)
            {
                double t = r.NextDouble();
                double tx = a[t] + b[t];
                double ty = c[t];
                error += Math.Abs(tx - ty);
            }


            CameraEntity camsensor = Entity.Camera();
            Entity cambody = Entity.Brush(Substance.Iron, Shape.Sphere(0.1)).Apply(new Transform(-0.12, 0.0, 0.0));
            Entity cam = camsensor.Embody(cambody);
            Entity obj = Entity.Brush(Substance.Iron, Shape.Sphere(1.0)).Apply(new Transform(5.0, 0.0, 0.0));
            Entity world = Entity.Combine(cam, obj);

            Span worldspan = Span.Create(world);

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.Control = new Visualizer(worldspan.Read(camsensor.Output).Simplify);
            hw.Run(60.0);
        }
    }
}