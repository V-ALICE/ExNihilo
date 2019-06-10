using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    //Basically an integral Vector2
    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate()
        {
            X = 0;
            Y = 0;
        }
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

        public Coordinate GetUp()
        {
            return new Coordinate(X, Y + 1);
        }
        public Coordinate GetDown()
        {
            return new Coordinate(X, Y - 1);
        }
        public Coordinate GetLeft()
        {
            return new Coordinate(X - 1, Y);
        }
        public Coordinate GetRight()
        {
            return new Coordinate(X + 1, Y);
        }

        public bool Equals(Coordinate check)
        {
            return X == check.X && Y == check.Y;
        }

        public Coordinate Copy()
        {
            return new Coordinate(X, Y);
        }

        public Vector2 AsVector2()
        {
            return new Vector2(X,Y);
        }

        public static Vector2 operator +(Coordinate a, Vector2 b)
        {
            return new Vector2(b.X + a.X, b.Y + a.Y);
        }
        public static Vector2 operator +(Vector2 a, Coordinate b)
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

        public bool IsRadial(Coordinate coord, int radius)
        {
            return Math.Abs(coord.X - X) <= radius && Math.Abs(coord.Y - Y) <= radius;
        }

        public bool IsDirectNeighbor(Coordinate coord)
        {
            return (Math.Abs(coord.X - X) <= 1 && coord.Y == Y) ||
                   (Math.Abs(coord.Y - Y) <= 1 && coord.X == X);
        }
    }
}
