using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace Alunite
{
    /// <summary>
    /// Functions related to the manipulation of interconnected points and triangles.
    /// </summary>
    public static class Model
    {
        /// <summary>
        /// Loads a model from an object file containing only location and face information.
        /// </summary>
        public static void LoadObj(Path Path, out ISequentialArray<Vector> Vertices, out ISequentialArray<Triangle<int>> Triangles)
        {
            using (FileStream fs = File.OpenRead(Path.PathString))
            {
                LoadObj(fs, out Vertices, out Triangles);
            }
        }

        /// <summary>
        /// Loads a model from a stream containing an object file containing only location and face information.
        /// </summary>
        public static void LoadObj(Stream Stream, out ISequentialArray<Vector> Vertices, out ISequentialArray<Triangle<int>> Triangles)
        {
            StreamReader sr = new StreamReader(Stream);
            ListArray<Vector> verts = new ListArray<Vector>(); Vertices = verts;
            ListArray<Triangle<int>> tris = new ListArray<Triangle<int>>(); Triangles = tris;

            // Read
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }

                string[] parts = line.Split(new char[] { ' ' });

                // Vertex
                if (parts[0] == "v")
                {
                    Vector vec = new Vector();
                    vec.X = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    vec.Y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    vec.Z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                    verts.Add(vec);
                }

                // Triangles
                if (parts[0] == "f")
                {
                    Triangle<int> tri = new Triangle<int>();
                    tri.A = int.Parse(parts[1], CultureInfo.InvariantCulture) - 1;
                    tri.B = int.Parse(parts[2], CultureInfo.InvariantCulture) - 1;
                    tri.C = int.Parse(parts[3], CultureInfo.InvariantCulture) - 1;
                    tris.Add(tri);
                }
            }
        }

    }

}