using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems
{
    public class TypeMatrix
    {
        public enum Type: byte
        {
            None, Wall, Ground
        }

        public readonly int X, Y;
        protected Type[][] _map;

        public TypeMatrix(Type[][] set)
        {
            X = set[0].Length;
            Y = set.Length;
            _map = set;
        }

        public TypeMatrix(string fileName)
        {
            var map = File.ReadAllLines("Content/Resources/" + fileName);
            if (map.Length == 0) return;
            X = map[0].Length;
            Y = map.Length;
            _map = new Type[Y][];
            for (int i = 0; i < Y; i++)
            {
                _map[i] = new Type[X];
                for (int j = 0; j < X; j++)
                {
                    switch (map[i][j])
                    {
                        case 'G':
                            _map[i][j] = Type.Ground;
                            break;
                        case 'W':
                            _map[i][j] = Type.Wall;
                            break;
                        default:
                            _map[i][j] = Type.None;
                            break;
                    }
                }
            }
        }

        public Type Get(int x, int y)
        {
            return _map[y][x];
        }
    }

    public class InteractionMap
    {
        public readonly TypeMatrix Map;
        private readonly Interactive[][] _interactive;
        private int _x => Map.X;
        private int _y => Map.Y;

        public InteractionMap(TypeMatrix mapIndex)
        {
            Map = mapIndex;
            _interactive = new Interactive[_y][];
            for (int i = 0; i < _y; i++)
            {
                _interactive[i] = new Interactive[_x];
                for (int j = 0; j < _x; j++)
                {
                    _interactive[i][j] = null;
                }
            }
        }

        public InteractionMap(string fileName)
        {
            Map = new TypeMatrix(fileName);
            _interactive = new Interactive[_y][];
            for (int i = 0; i < _y; i++)
            {
                _interactive[i] = new Interactive[_x];
                for (int j = 0; j < _x; j++)
                {
                    _interactive[i][j] = null;
                }
            }
        }

        public Coordinate GetAnyFloor(Random rand)
        {
            //TODO: this can get unlucky and spin for a long time potentially, find a better way to do it
            while (true)
            {
                var x = rand.Next(_x);
                var y = rand.Next(_y);
                var t = Map.Get(x, y);
                if (t == TypeMatrix.Type.Ground) return new Coordinate(x, y);
            }
        }

        public void AddInteractive(Interactive obj, int x, int y, int width, int height)
        {
            //starts from top left
            for (int i = y; i < y+height && i < _y && i >= 0; i++)
            {
                for (int j = x; j < x+width && j < _x && j >= 0; j++)
                {
                    _interactive[i][j] = obj;
                }
            }
        }

        public bool CheckIllegalPosition(int scaledTileSize, Coordinate scaledHitBox, Vector2 scaledHitBoxOffsetFromWorld)
        {
            var minX = MathD.RoundDown(scaledHitBoxOffsetFromWorld.X / scaledTileSize);
            var minY = MathD.RoundDown(scaledHitBoxOffsetFromWorld.Y / scaledTileSize);
            var maxX = MathD.RoundDown((scaledHitBoxOffsetFromWorld.X + scaledHitBox.X) / scaledTileSize);
            var maxY = MathD.RoundDown((scaledHitBoxOffsetFromWorld.Y + scaledHitBox.Y) / scaledTileSize);

            for (int i = minY; i <= maxY && i < _y && i >= 0; i++)
            {
                for (int j = minX; j <= maxX && j < _x && j >= 0; j++)
                {
                    if (Map.Get(j, i) != TypeMatrix.Type.Ground) return true;
                }
            }

            return false;
        }

        public Interactive GetInteractive(int scaledTileSize, Vector2 offsetFromWorld, int radius)
        {
            var x = MathD.RoundDown(offsetFromWorld.X / scaledTileSize);
            var y = MathD.RoundDown(offsetFromWorld.Y / scaledTileSize);

            for (int i = y - radius; i <= y + radius; i++)
            {
                if (i >= _y || i < 0) continue;
                for (int j = x - radius; j <= x + radius && j < _x && j >= 0; j++)
                {
                    if (j >= _x || j < 0) continue;
                    if (_interactive[i][j] != null) return _interactive[i][j];
                }
            }

            return null;
        }

    }
}
