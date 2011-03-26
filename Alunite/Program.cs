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
            CubicSignal<double, ScalarContinuum> sig = CubicSignal<double, ScalarContinuum>.Linear(new ScalarContinuum(), 30.0, 0.0, 1.0);
            double f = sig[0.5];

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