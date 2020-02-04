
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

            public TileNode(int x, int y, int proc, Random rand)
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

            public TileNode Add(int x, int y, int proc, Random rand)
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

        private static void GetRandomRoom(TileNodeSet map, Random rand, int proc=40)
        {
            void Push(TileNode point, int xDiff, int yDiff)
            {
                if (point.X + xDiff <= 1 || point.X + xDiff >= map.Length-1) return;
                if (point.Y + yDiff <= 1 || point.Y + yDiff >= map.Length-1) return;
                if (map.Get(point.X + xDiff, point.Y + yDiff).Real) return;

                var next = map.Add(point.X + xDiff, point.Y+yDiff, proc, rand);
                if (next.Up) Push(next, 0, 1);
                if (next.Down) Push(next, 0, -1);
                if (next.Left) Push(next, -1, 0);
                if (next.Right) Push(next, 1, 0);
            }

            var origin = map.Add(map.Length / 2, map.Length / 2, proc, rand);
            Push(origin, 0, 1);
            Push(origin, 0, -1);
            Push(origin, -1, 0);
            Push(origin, 1, 0);
        }

        private static void GetRandom(TileNodeSet map, Random rand)
        {
            GetRandomRoom(map, rand);
            //TODO
        }

        private static void GetBoxes(TileNodeSet map, Random rand)
        {
            //TODO
        }

        private static void GetInverted(TileNodeSet map, Random rand)
        {
            //TODO
        }

        public static Tile[][] Get(int key, int subKey, Type type=Type.Random, int setSize=1024)
        {
            var map = new TileNodeSet(setSize);
            var rand = new Random(key + subKey);
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
