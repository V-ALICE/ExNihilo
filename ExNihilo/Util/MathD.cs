using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    public class SubKeyRandom : Random
    {
        private readonly int _off;

        public SubKeyRandom(int seed, int offset) : base(seed)
        {
            _off = offset;
        }

        public override int Next(int minValue, int maxValue)
        {
            return (base.Next(minValue, maxValue) + _off) % maxValue + minValue;
        }

        public override int Next(int maxValue)
        {
            return (base.Next(maxValue) + _off) % maxValue;
        }

        public override int Next()
        {
            return base.Next() + _off;
        }
    }

    public static class MathD
    {
        //Universal random for any non-seeded non-strict operations
        public static Random urand = new Random();

        //Relative equality of double/float and Vectors 
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

        //rounds x to an integer towards negative infinity
        public static int RoundDown(double x, double tolerance = 1e-6)
        {
            if (Math.Abs(x) < tolerance) return 0;
            if (x > 0) return (int)x;
            return (int)(x - 1);
        }

        //Produces a number in the specified range with a 50% change to be positive/negative
        public static int RandomlySigned(Random r, int min, int max)
        {
            return (r.Next(0, 2) == 0 ? -1 : 1)*r.Next(min, max);
        }

        //produced true with a [chance/total]% chance
        public static bool Chance(Random r, int chance, int total=100)
        {
            return r.Next(total) < chance;
        }
    }
}
