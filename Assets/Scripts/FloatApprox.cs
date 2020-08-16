using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class FloatApprox
{
    public static bool Approx(this float a, in float b, in int precision = 2) {
        double d = System.Math.Round(b, precision);
        return System.Math.Round(a, precision) == d;
    }

    public static bool Tolerance(this float a, in float b, in float precision = .1f)
    {
        return a >= b - precision && a <= b + precision;
    }

    public static bool IsZero(this float a) => Mathf.Approximately(a, 0);
}