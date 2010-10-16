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
        public static void LoadObj(Path Path, out IArray<Vector> Vertices, out IArray<Triangle<int>> Triangles)
        {
            using (FileStream fs = File.OpenRead(Path.PathString))
            {
                LoadObj(fs, out Vertices, out Triangles);
            }
        }

        /// <summary>
        /// Loads a model from a stream containing an object file containing only location and face information.
        /// </summary>
        public static void LoadObj(Stream Stream, out IArray<Vector> Vertices, out IArray<Triangle<int>> Triangles)
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

        /// <summary>
        /// Computes the normals for the specified set of vertices and triangles. The indices for the normals are aligned with vertices.
        /// </summary>
        public static StandardArray<Vector> ComputeNormals(IArray<Vector> Vertices, IEnumerable<Triangle<int>> Triangles, bool Normalize)
        {
            Vector[] normals = new Vector[Vertices.Size];
            foreach (Triangle<int> tri in Triangles)
            {
                Triangle<Vector> vectri = new Triangle<Vector>(Vertices.Lookup(tri.A), Vertices.Lookup(tri.B), Vertices.Lookup(tri.C));
                Vector norm = Triangle.Normal(vectri);
                normals[tri.A] += norm;
                normals[tri.B] += norm;
                normals[tri.C] += norm;
            }
            if (Normalize)
            {
                for (int t = 0; t < normals.Length; t++)
                {
                    normals[t] = Vector.Normalize(normals[t]);
                }
            }
            return new StandardArray<Vector>(normals);
        }
    }
}