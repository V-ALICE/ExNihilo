using System;
using System.Collections.Generic;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Tile = ExNihilo.Systems.Backend.TypeMatrix.Type;

namespace ExNihilo.Systems.Game
{
    public static class MapGenerator
    {
        public enum Type
        {
            MessyBoxes,   // Rectangular rooms
            Standard1,    // MessyBoxes with some clean up applied
            Standard2,    // Standard1 with wider halls and less spaced out rooms 
        }

        private class TileNodeSet
        {
            private readonly bool[][] _map;
            public int MinX, MinY, MaxX, MaxY;

            public TileNodeSet(int size)
            {
                _map = new bool[size][];
                for (int i = 0; i < size; i++) _map[i] = new bool[size];
                MinX = size - 1;
                MinY = size - 1;
                MaxX = 0;
                MaxY = 0;
            }

            public bool Get(int x, int y)
            {
                if (x < 0 || y < 0 || x >= _map.Length || y >= _map.Length) return false;
                return _map[y][x];
            }

            public void Add(int x, int y)
            {
                if (x < MinX) MinX = x;
                if (x > MaxX) MaxX = x;
                if (y < MinY) MinY = y;
                if (y > MaxY) MaxY = y;
                _map[y][x] = true;
            }
            public void AddAll(int x, int y, int dx, int dy)
            {
                var xDir = Math.Sign(dx);
                var yDir = Math.Sign(dy);
                if (dx == 0)
                {
                    for (int j = y; j != y + dy + yDir; j += yDir)
                    {
                        Add(x, j);
                    }
                }
                else if (dy == 0)
                {
                    for (int i = x; i != x + dx + xDir; i += xDir)
                    {
                        Add(i, y);
                    }
                }
                else
                {
                    for (int i = x; i != x + dx + xDir; i += xDir)
                    {
                        for (int j = y; j != y + dy + yDir; j += yDir)
                        {
                            Add(i, j);
                        }
                    }
                }
            }

            public bool IsAreaClear(int x, int y, int dx, int dy)
            {
                var xDir = Math.Sign(dx);
                var yDir = Math.Sign(dy);
                if (dx == 0)
                {
                    for (int j = y; j != y + dy + yDir; j += yDir)
                    {
                        if (_map[j][x]) return false;
                    }
                }
                else if (dy == 0)
                {
                    for (int i = x; i != x + dx + xDir; i += xDir)
                    {
                        if (_map[y][i]) return false;
                    }
                }
                else
                {
                    for (int i = x; i != x + dx + xDir; i += xDir)
                    {
                        for (int j = y; j != y + dy + yDir; j += yDir)
                        {
                            if (_map[j][i]) return false;
                        }
                    }
                }

                return true;
            }

            public int Length => _map.Length;
        }

        private static void GetBoxes(TileNodeSet map, Random rand, int minSize, int maxSize, int totalRooms, int roomSepMin, int hallWidth)
        {
            // up=0, down=1, left=2, right=3
            void ConnectRooms(Rectangle a, Rectangle b, int width=2)
            {
                var w = MathHelper.Clamp(width - 1, 1, minSize-2);
                var start = new Coordinate(rand.Next(a.Left, a.Right-w), rand.Next(a.Top, a.Bottom-w));
                var end = new Coordinate(rand.Next(b.Left, b.Right-w), rand.Next(b.Top, b.Bottom-w));
                var xw = end.X - start.X < 0 ? 0 : w;

                if (start.X != end.X) map.AddAll(start.X, start.Y, end.X - start.X + xw, w);
                if (start.Y != end.Y) map.AddAll(end.X, start.Y, w, end.Y - start.Y);
            }

            var todoList = new List<Rectangle>();
            var attempts = 0;
            while (todoList.Count < totalRooms && attempts++ < totalRooms*10)
            {
                var x = rand.Next(roomSepMin, map.Length - maxSize - roomSepMin);
                var y = rand.Next(roomSepMin, map.Length - maxSize - roomSepMin);
                var width = rand.Next(minSize, maxSize);
                var height = rand.Next(minSize, maxSize);
                if (map.IsAreaClear(x - roomSepMin, y - roomSepMin, width + 2 * roomSepMin, height + 2 * roomSepMin))
                {
                    todoList.Add(new Rectangle(x, y, width, height));
                    map.AddAll(x, y, width, height);
                }
            }

            for (int i = 0; i < todoList.Count; i++)
            {
                int another;
                do another = rand.Next(todoList.Count);
                while (i == another);
                ConnectRooms(todoList[i], todoList[another], hallWidth);
            }
        }

        private static void CleanWallBits(Tile[][] set)
        {
            for (int x = 1; x < set[0].Length - 2; x++)
            {
                for (int y = 1; y < set.Length - 2; y++)
                {
                    if (x < 1) x = 1;
                    if (y < 1) y = 1;
                    if (set[y][x] == Tile.Wall)
                    {
                        if (set[y][x - 1] == Tile.Ground)
                        {
                            if (set[y][x + 1] == Tile.Ground)
                            {
                                // ground wall ground horizontal
                                set[y--][x--] = Tile.Ground;
                                continue;
                            }
                            if (set[y][x + 1] == Tile.Wall && set[y][x + 2] == Tile.Ground)
                            {
                                // ground wall wall ground horizontal
                                set[y][x] = Tile.Ground;
                                set[y--][x-- + 1] = Tile.Ground;
                                continue;
                            }
                        }

                        if (set[y - 1][x] == Tile.Ground)
                        {
                            if (set[y + 1][x] == Tile.Ground)
                            {
                                // ground wall ground vertical
                                set[y--][x--] = Tile.Ground;
                                continue;
                            }
                            if (set[y + 1][x] == Tile.Wall && set[y + 2][x] == Tile.Ground)
                            {
                                // ground wall wall ground vertical
                                set[y][x] = Tile.Ground;
                                set[y-- + 1][x--] = Tile.Ground;
                            }
                        }
                    }
                }
            }
        }

        private static volatile int _gTotal;
        public static Tile[][] Get(int key, int subKey, Type type, out Random rand)
        {
            TileNodeSet map;
            rand = new Random(key);

            //Prep random with subKey
            for (int i = 0; i < subKey; i++) _gTotal += rand.Next(1) + _gTotal;

            switch (type)
            {
                case Type.MessyBoxes:
                case Type.Standard1:
                    map = new TileNodeSet(128);
                    GetBoxes(map, rand, 12, 25, 12, 3, 2);
                    break;
                case Type.Standard2:
                default:
                    map = new TileNodeSet(128);
                    GetBoxes(map, rand, 12, 25, 12, 0, 4);
                    break;
            }

            //Transfer bitmap into constricted tile map and add walls 
            var set = new Tile[map.MaxY - map.MinY + 3][];
            for (int i = map.MinY - 1; i <= map.MaxY + 1; i++)
            {
                var curY = i - map.MinY + 1;
                set[curY] = new Tile[map.MaxX - map.MinX + 3];
                for (int j = map.MinX - 1; j <= map.MaxX + 1; j++)
                {
                    var curX = j - map.MinX + 1;
                    if (map.Get(j, i)) set[curY][curX] = Tile.Ground;
                    else
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                if (map.Get(j + x, i + y)) set[curY][curX] = Tile.Wall;
                            }
                        }
                    }
                }
            }

            //Post processing
            if (type == Type.Standard1 || type == Type.Standard2)
            {
                CleanWallBits(set);
            }

            return set;
        }
    }
}
