using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    //Basically an integral Vector2
    public struct Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Coordinate(float x, float y)
        {
            X = MathD.RoundDown(x);
            Y = MathD.RoundDown(y);
        }
        public Coordinate(Vector2 v)
        {
            X = MathD.RoundDown(v.X);
            Y = MathD.RoundDown(v.Y);
        }

        public static explicit operator Coordinate(Vector2 v)
        {
            return new Coordinate(v);
        }
        public static explicit operator Vector2(Coordinate cv)
        {
            return new Vector2(cv.X, cv.Y);
        }

        public bool Equals(Coordinate check)
        {
            return X == check.X && Y == check.Y;
        }

        public bool Origin()
        {
            return X == 0 && Y == 0;
        }

        public Coordinate Copy()
        {
            return new Coordinate(X, Y);
        }

        public static Coordinate operator +(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.X + b.X, a.Y + b.Y);
        }

        public static Coordinate operator -(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator +(Coordinate a, Vector2 b)
        {
            return new Vector2(b.X + a.X, b.Y + a.Y);
        }
        public static Vector2 operator -(Vector2 a, Coordinate b)
        {
            return new Vector2(a.X-b.X, a.Y-b.Y);
        }
        public static Vector2 operator -(Coordinate a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }
        public static Vector2 operator *(Coordinate a, Vector2 b)
        {
            return new Vector2(a.X * b.X, a.Y * b.Y);
        }
        public static Vector2 operator *(Vector2 a, Coordinate b)
        {
            return new Vector2(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2 operator *(float a, Coordinate b)
        {
            return new Vector2(a*b.X, a*b.Y);
        }

        public override string ToString()
        {
            return "X:" + X + " Y:" + Y;
        }
    }
}
