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

            // Create point set
            _SphereTree st = new _SphereTree();
            var points = new List<SimpleSphereTreeNode<Vector>>();
            for (int t = 0; t < 1000000; t++)
            {
                points.Add(st.Create(new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble())));
            }
            SimpleSphereTreeNode<Vector> node = st.Create(points);
            return;

            /*
            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(world);
            hw.Run(60.0);*/
        }

        private class _SphereTree : SimpleSphereTree<Vector>
        {
            public override void GetBound(Vector Leaf, out Vector Position, out double Radius)
            {
                Position = Leaf;
                Radius = 0.0;
            }
        }
    }
}