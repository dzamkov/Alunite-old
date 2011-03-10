using System;
using System.Collections.Generic;

using OpenTK;
using OpenTKGUI;

using Alunite.Fast;

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
            Curve<Scalar> t = new Curve<Scalar>(new Scalar[] { 1.0, -(67.0 / 81.0), (26.0 / 81.0), 0.0 });
            double s = t[1.0 / 3.0];
            double q = t[2.0 / 3.0];

            Curve<Scalar> test = Curve.Match(new Scalar[] { 0.0, 1.0, 2.0, 1.0, 0.0 });
            double a = test[0.0];
            double b = test[0.25];
            double c = test[0.5];
            double d = test[0.75];
            double e = test[1.0];

            Curve<Vector> acurve = Curve.Linear(new Vector(-1.0, 0.0, 0.5), new Vector(1.0, 0.0, 0.5));
            Curve<Vector> bcurve = Curve.Linear(new Vector(0.0, 1.0, 0.0), new Vector(0.0, -1.0, 0.0));
            Curve<Scalar> dist = Curve.Distance(acurve, bcurve);
        }
    }
}