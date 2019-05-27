using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    public static class MathD
    {
        public static bool CheckClose(double a, double b, double tolerance = 1e-6)
        {
            return Math.Abs(a - b) < tolerance;
        }

        public static bool CheckClose(Vector2 a, Vector2 b, double tolerance = 1e-6)
        {
            return CheckClose(a.X, b.X) && CheckClose(a.Y, b.Y);
        }

        public static int RoundDown(double x, double tolerance = 1e-6)
        {
            if (Math.Abs(x) < tolerance) return 0;
            if (x > 0) return (int)x;
            return (int)(x - 1);
        }
    }
}
