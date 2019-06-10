using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    public static class Utilities
    {
        public static int GetAbsoluteSeed(Random rand, string seed)
        {
            if (int.TryParse(seed, out int numeral)) return numeral;
            return string.IsNullOrEmpty(seed) ? rand.Next() : seed.GetHashCode();
        }

        public static Vector2 Copy(Vector2 inV)
        {
            return new Vector2(inV.X, inV.Y);
        }

        public static string Clamp(string input, int max)
        {
            return input.Length > max ? input.Substring(0, max) : input;
        }

    }
}
