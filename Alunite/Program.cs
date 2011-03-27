﻿using System;
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
            CubicSignal<double, ScalarContinuum> sig = new CubicSignal<double, ScalarContinuum>(
                new CubicSignal<double>.Vertex[]
                {
                    new CubicSignal<double>.Vertex(0.0, 0.0, 0.0),
                    new CubicSignal<double>.Vertex(1.0, 1.0, 0.0)
                }, new ScalarContinuum());

            Random r = new Random();
            for (int i = 0; i < 100; i++)
            {
                double t = r.NextDouble();
                const double h = 0.00001;
                double d = sig.GetDerivative(t);
                double a = sig[t];
                double b = sig[t + h];
                double nd = (b - a) / h;
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