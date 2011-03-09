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
            Curve<Scalar> acurve = Curve.Linear(0.0, 1.0);
            Curve<Scalar> bcurve = Curve.Linear(1.0, 0.4);
            acurve = Curve.Integral(acurve, (Scalar)0.1);
            bcurve = Curve.Integral(bcurve, (Scalar)0.0);
            Curve<Scalar> pcurve = Curve.Multiply(acurve, bcurve);
            double p = 0.4;
            double ab = acurve[p] * bcurve[p];
            double tn = pcurve[p];
        }
    }
}