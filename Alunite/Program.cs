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
            Random r = new Random();

            // OSP test
            List<_OSPNode> nodes = new List<_OSPNode>();
            for (int t = 0; t < 100000; t++)
            {
                nodes.Add(new _OSPNode(r.NextDouble() * 0.01, new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble())));
            }
            _OSPNode ospnode = OSP.Create<_OSPNode.Input, _OSPNode, double>(new _OSPNode.Input(), nodes);

            // Create a test world
            List<Matter> elems = new List<Matter>();

            BlobMatter worldblob = new BlobMatter(0.1);

            // Water
            for (int t = 0; t < 10000; t++)
            {
                worldblob.Add(new Particle(
                    new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble() * 0.5),
                    0.01, Fluid.GetSubstance()));
            }

            // Adminium walls
            double step = 0.02;
            for (double x = step / 2.0; x <= 1.0; x += step)
            {
                for (double y = step / 2.0; y <= 1.0; y += step)
                {
                    worldblob.Add(new Particle(new Vector(x, y, 0.0), 0.01, new Adminium()));
                    worldblob.Add(new Particle(new Vector(x, 0.0, y), 0.01, new Adminium()));
                    worldblob.Add(new Particle(new Vector(0.0, x, y), 0.01, new Adminium()));
                    worldblob.Add(new Particle(new Vector(x, 1.0, y), 0.01, new Adminium()));
                    worldblob.Add(new Particle(new Vector(1.0, x, y), 0.01, new Adminium()));
                }
            }

            // "Earth"
            elems.Add(worldblob);
            elems.Add(new Particle(
                new Vector(0.0, 0.0, -6.3675e6),
                5.9721e24, new Adminium()).Matter);

            Matter world = CompositeMatter.Create(elems);

            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(world);
            hw.Run(60.0);
        }

        private class _OSPNode
        {
            public _OSPNode(double Radius, Vector Position)
            {
                this.Radius = Radius;
                this.Position = Position;
            }

            public _OSPNode(_OSPNode A, _OSPNode B)
            {
                Vector dir = B.Position - A.Position;
                double dis = dir.Length;
                dir *= 1.0 / dis;
                this.Radius = (dis + A.Radius + B.Radius) * 0.5;
                this.Position = A.Position + dir * (this.Radius - A.Radius);
                this.SubA = A;
                this.SubB = B;
            }

            public class Input : OSP.IOSPInput<_OSPNode, double>
            {
                public bool GetSubnodes(_OSPNode Node, out _OSPNode A, out _OSPNode B)
                {
                    if (Node.SubA != null)
                    {
                        A = Node.SubA;
                        B = Node.SubB;
                        return true;
                    }
                    else
                    {
                        A = null;
                        B = null;
                        return false;
                    }
                }

                public bool Greater(double A, double B)
                {
                    return A > B;
                }

                public double GetDiameter(_OSPNode Node)
                {
                    return Node.Radius * 2.0;
                }

                public double GetShortDistance(_OSPNode A, _OSPNode B)
                {
                    return (A.Position - B.Position).Length - A.Radius - B.Radius;
                }

                public double GetLongDistance(_OSPNode A, _OSPNode B)
                {
                    return (A.Position - B.Position).Length + A.Radius + B.Radius;
                }

                public _OSPNode CreateCompound(_OSPNode A, _OSPNode B)
                {
                    return new _OSPNode(A, B);
                }
            }

            public double Radius;
            public Vector Position;
            public _OSPNode SubA;
            public _OSPNode SubB;
        }
    }
}