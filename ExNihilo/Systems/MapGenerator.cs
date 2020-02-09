
using System;
using System.Collections.Generic;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tile = ExNihilo.Systems.TypeMatrix.Type;

namespace ExNihilo.Systems
{
    public static class MapGenerator
    {
        public enum Type
        {
            Random,   // Rooms like the previous version with no particular shape
            Boxes,    // Rectangular rooms
            Inverted, // Big open space with random holes
        }

        private struct TileNode
        {
            public readonly int X, Y;
            public readonly bool Up, Down, Left, Right;
            public readonly bool Real;

            public TileNode(int x, int y)
            {
                Real = true;
                X = x;
                Y = y;
                Up = false;
                Down = false;
                Left = false;
                Right = false;
            }
            public TileNode(int x, int y, int proc, SubKeyRandom rand)
            {
                Real = true;
                X = x;
                Y = y;
                Up = MathD.Chance(rand, proc);
                Down = MathD.Chance(rand, proc);
                Left = MathD.Chance(rand, proc);
                Right = MathD.Chance(rand, proc);
            }
        }

        private class TileNodeSet
        {
            private TileNode[][] _map;
            public int MinX, MinY, MaxX, MaxY;

            public TileNodeSet(int size)
            {
                _map = new TileNode[size][];
                for (int i = 0; i < size; i++) _map[i] = new TileNode[size];
                MinX = size - 1;
                MinY = size - 1;
                MaxX = 0;
                MaxY = 0;
            }

            public TileNode Get(int x, int y)
            {
                return _map[y][x];
            }

            public TileNode Add(int x, int y)
            {
                if (x < MinX) MinX = x;
                if (x > MaxX) MaxX = x;
                if (y < MinY) MinY = y;
                if (y > MaxY) MaxY = y;
                _map[y][x] = new TileNode(x, y);
                return _map[y][x];
            }
            public TileNode Add(int x, int y, int proc, SubKeyRandom rand)
            {
                if (x < MinX) MinX = x;
                if (x > MaxX) MaxX = x;
                if (y < MinY) MinY = y;
                if (y > MaxY) MaxY = y;
                _map[y][x] = new TileNode(x, y, proc, rand);
                return _map[y][x];
            }

            public int Length => _map.Length;
        }

        private static void GetRandomChunk(TileNodeSet map, SubKeyRandom rand, Rectangle space, int proc=49)
        {
            void Push(TileNode point, int xDiff, int yDiff)
            {
                if (point.X + xDiff <= space.Left || point.X + xDiff >= space.Right-1) return;
                if (point.Y + yDiff <= space.Top || point.Y + yDiff >= space.Bottom-1) return;
                if (map.Get(point.X + xDiff, point.Y + yDiff).Real) return;

                var next = map.Add(point.X + xDiff, point.Y+yDiff, proc, rand);
                if (next.Up) Push(next, 0, 1);
                if (next.Down) Push(next, 0, -1);
                if (next.Left) Push(next, -1, 0);
                if (next.Right) Push(next, 1, 0);
            }

            var origin = map.Add(space.X+space.Width / 2, space.Y+space.Height / 2);
            Push(origin, 0, 1);
            Push(origin, 0, -1);
            Push(origin, -1, 0);
            Push(origin, 1, 0);
        }

        private static void GetRandom(TileNodeSet map, SubKeyRandom rand, int chunkSize=32)
        {
            var count = map.Length / chunkSize;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    GetRandomChunk(map, rand, new Rectangle(j*chunkSize, i*chunkSize, chunkSize, chunkSize));
                }
            }
        }

        private static void GetBoxes(TileNodeSet map, SubKeyRandom rand)
        {
            //TODO
        }

        private static void GetInverted(TileNodeSet map, SubKeyRandom rand)
        {
            //TODO
        }

        public static Tile[][] Get(int key, int subKey, Type type, int setSize)
        {
            if (setSize > 256) setSize = 256;
            var map = new TileNodeSet(setSize);
            var rand = new SubKeyRandom(key, subKey);
            switch (type)
            {
                case Type.Random:
                    GetRandom(map, rand);
                    break;
                case Type.Boxes:
                    GetBoxes(map, rand);
                    break;
                case Type.Inverted:
                    GetInverted(map, rand);
                    break;
            }

            var set = new Tile[map.MaxY - map.MinY+3][];
            for (int i = map.MinY-1; i <= map.MaxY+1; i++)
            {
                set[i-map.MinY+1] = new Tile[map.MaxX - map.MinX+3];
                for (int j = map.MinX-1; j <= map.MaxX+1; j++)
                {
                    if (map.Get(j, i).Real) set[i-map.MinY+1][j-map.MinX+1] = Tile.Ground;
                    else
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                if (j+x < 0 || j+x >= map.Length || i+y < 0 || i+y >= map.Length) continue;
                                if (map.Get(j+x, i+y).Real) set[i-map.MinY+1][j-map.MinX+1] = Tile.Wall;
                            }
                        }
                    }
                }
            }

            return set;
        }
    }
}
