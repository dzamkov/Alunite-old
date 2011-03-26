using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains functions for manipulating and evaluating b-splines.
    /// </summary>
    public static class BSpline
    {
        /// <summary>
        /// Evaluates a bspline defines by an a knot vector and a control point vector at a certain parameter.
        /// </summary>
        public static T Evaluate<T, TInterpolation>(TInterpolation Interpolation, double[] Knots, T[] Points, int Degree, double Parameter)
            where TInterpolation : IInterpolation<T>
        {
            int l = GetInterval(Knots, Parameter);

            T[] temp = new T[Degree + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = Points[l - i];
            }
            for (int k = 0; k < Degree; k++)
            {
                for (int m = 0; m < Degree - k; m++)
                {
                    int u = l - m;
                    double ui = Knots[u];
                    double a = (Parameter - ui) / (Knots[u + Degree - k] - ui);
                    temp[m] = Interpolation.Mix(temp[m + 1], temp[m], a);
                }
            }

            return temp[0];
        }

        /// <summary>
        /// Evaluates an open b-spline such that the start and end parameters will return the start and end points respectively. The start parameter is assumed to be 0.0.
        /// The amount of inner knots should be equal to (Points.Length - Degree - 1).
        /// </summary>
        public static T EvaluateOpen<T, TInterpolation>(TInterpolation Interpolation, double[] InnerKnots, double End, T[] Points, int Degree, double Parameter)
            where TInterpolation : IInterpolation<T>
        {
            int l = (InnerKnots.Length == 0 || Parameter < InnerKnots[0]) ? Degree : (GetInterval(InnerKnots, Parameter) + 1 + Degree);

            T[] temp = new T[Degree + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = Points[l - i];
            }
            for (int k = 0; k < Degree; k++)
            {
                for (int m = 0; m < Degree - k; m++)
                {
                    int u = l - m - Degree - 1;
                    double ui = _LookupInnerKnot(InnerKnots, End, u);
                    double a = (Parameter - ui) / (_LookupInnerKnot(InnerKnots, End, u + Degree - k) - ui);
                    temp[m] = Interpolation.Mix(temp[m + 1], temp[m], a);
                }
            }

            return temp[0];
        }

        private static double _LookupInnerKnot(double[] InnerKnots, double End, int Knot)
        {
            if (Knot < 0) return 0.0;
            if (!(Knot < InnerKnots.Length)) return End;
            return InnerKnots[Knot];
        }

        /// <summary>
        /// Gets the highest interval (index of a knot) whose value is before the given parameter.
        /// </summary>
        public static int GetInterval(double[] Knots, double Parameter)
        {
            int l = 0;
            int h = Knots.Length;
            while (h > l + 1)
            {
                int s = (l + h) / 2;
                if (Parameter >= Knots[s])
                {
                    l = s;
                }
                else
                {
                    h = s;
                }
            }
            return l;
        }
    }
}