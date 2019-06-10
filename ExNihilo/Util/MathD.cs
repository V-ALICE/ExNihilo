using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    public static class MathD
    {
        public static bool IsClose(double a, double b, double tolerance = 1e-6)
        {
            return Math.Abs(a - b) < tolerance;
        }
        public static bool IsClose(float a, float b, float tolerance = 1e-6f)
        {
            return Math.Abs(a - b) < tolerance;
        }
        public static bool IsClose(Vector2 a, Vector2 b, float tolerance = 1e-6f)
        {
            return IsClose(a.X, b.X) && IsClose(a.Y, b.Y);
        }

        public static int RoundDown(double x, double tolerance = 1e-6)
        {
            if (Math.Abs(x) < tolerance) return 0;
            if (x > 0) return (int)x;
            return (int)(x - 1);
        }

        public static int RandomlySigned(Random r, int min, int max)
        {
            return (r.Next(0, 2) == 0 ? -1 : 1)*r.Next(min, max);
        }
    }
}
