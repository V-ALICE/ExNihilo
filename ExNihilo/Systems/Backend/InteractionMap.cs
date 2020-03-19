using System;
using System.Collections.Generic;
using System.IO;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Backend
{
    public class TypeMatrix
    {
        public enum Type: byte
        {
            None, Wall, Ground, Stairs, Box
        }

        public readonly int X, Y;
        protected Type[][] Map;

        public TypeMatrix(Type[][] set)
        {
            X = set[0].Length;
            Y = set.Length;
            Map = set;
        }

        public TypeMatrix(string[] map)
        {
            if (map.Length == 0) return;
            X = map[0].Length;
            Y = map.Length;
            Map = new Type[Y][];
            for (int i = 0; i < Y; i++)
            {
                Map[i] = new Type[X];
                for (int j = 0; j < X; j++)
                {
                    switch (map[i][j])
                    {
                        case 'O':
                            Map[i][j] = Type.Ground;
                            break;
                        case 'X':
                            Map[i][j] = Type.Wall;
                            break;
                        default:
                            Map[i][j] = Type.None;
                            break;
                    }
                }
            }
        }

        public Type Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= X || y >= Y) return Type.None;
            return Map[y][x];
        }

        public void Force(int x, int y, Type type)
        {
            //This should be used sparingly
            if (x < 0 || y < 0 || x >= X || y >= Y) return;
            Map[y][x] = type;
        }

        public bool IsFreeFloor(int x, int y)
        {
            //A free floor is a floor that is surrounded by 8 other floor tiles
            //AKA a tile that can be replaced with a wall with no issue guaranteed 
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (Get(x + i, y + j) != Type.Ground) return false;
                }
            }

            return true;
        }
    }

    public class InteractionMap
    {
        public readonly TypeMatrix Map;
        private readonly Interactive[][] _interactive;
        private readonly List<Coordinate> _boxes = new List<Coordinate>();
        private int X => Map.X;
        private int Y => Map.Y;

        public InteractionMap(TypeMatrix mapIndex)
        {
            Map = mapIndex;
            _interactive = new Interactive[Y][];
            for (int i = 0; i < Y; i++)
            {
                _interactive[i] = new Interactive[X];
                for (int j = 0; j < X; j++)
                {
                    _interactive[i][j] = null;
                }
            }
        }

        public InteractionMap(string[] map)
        {
            Map = new TypeMatrix(map);
            _interactive = new Interactive[Y][];
            for (int i = 0; i < Y; i++)
            {
                _interactive[i] = new Interactive[X];
                for (int j = 0; j < X; j++)
                {
                    _interactive[i][j] = null;
                }
            }
        }

        public void OverwriteTile(int x, int y, TypeMatrix.Type type)
        {
            Map.Force(x, y, type);
        }

        public Coordinate GetAnyFreeFloor(Random rand)
        {
            var list = new List<int>();
            while (true)
            {
                list.Clear();
                var row = rand.Next(Y);
                for (int i = 0; i < X; i++)
                {
                    if (Map.IsFreeFloor(row, i) && GetInteractive(row, i) is null) list.Add(i);
                }

                if (list.Count > 0) return new Coordinate(row, list[rand.Next(list.Count)]);
            }
        }
        public Coordinate GetAnyFloor(Random rand)
        {
            var list = new List<int>();
            while (true)
            {
                list.Clear();
                var row = rand.Next(Y);
                for (int i = 0; i < X; i++)
                {
                    if (Map.Get(row, i) == TypeMatrix.Type.Ground) list.Add(i);
                }

                if (list.Count > 0) return new Coordinate(row, list[rand.Next(list.Count)]);
            }
        }

        public void AddInteractive(Interactive obj, int x, int y, int width, int height)
        {
            //starts from top left
            for (int i = y; i < y+height && i < Y && i >= 0; i++)
            {
                for (int j = x; j < x+width && j < X && j >= 0; j++)
                {
                    _interactive[i][j] = obj;
                }
            }
        }

        public void AddBox(BoxInteractive box, int x, int y)
        {
            box.Index = _boxes.Count;
            AddInteractive(box, x, y, 1, 1);

            _boxes.Add(new Coordinate(x, y));
        }

        public void RemoveFromBox(int index, int id)
        {
            try
            {
                var loc = _boxes[index];
                var box = _interactive[loc.Y][loc.X] as BoxInteractive;
                box?.Remove(id);
            }
            catch (IndexOutOfRangeException)
            {
                SystemConsole.ForceMessage("<error>", "Attempted to remove item from box that does not exist", Color.DarkRed, Color.White);
            }
        }

        public void DrawBoxes(SpriteBatch spriteBatch, Vector2 offset, int tileSize, float scale)
        {
            foreach (var b in _boxes)
            {
                var box = _interactive[b.Y][b.X] as BoxInteractive;
                if (box is null) continue;
                spriteBatch.Draw(box.GetTexture(), scale*tileSize * b + offset, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
        }

        public bool CheckIllegalPosition(int scaledTileSize, Coordinate scaledHitBox, Vector2 scaledHitBoxOffsetFromWorld)
        {
            var minX = MathD.RoundDown(scaledHitBoxOffsetFromWorld.X / scaledTileSize);
            var minY = MathD.RoundDown(scaledHitBoxOffsetFromWorld.Y / scaledTileSize);
            var maxX = MathD.RoundDown((scaledHitBoxOffsetFromWorld.X + scaledHitBox.X) / scaledTileSize);
            var maxY = MathD.RoundDown((scaledHitBoxOffsetFromWorld.Y + scaledHitBox.Y) / scaledTileSize);

            for (int i = minY; i <= maxY && i < Y && i >= 0; i++)
            {
                for (int j = minX; j <= maxX && j < X && j >= 0; j++)
                {
                    var tile = Map.Get(j, i);
                    if (tile != TypeMatrix.Type.Ground && tile != TypeMatrix.Type.Stairs) return true;
                }
            }

            return false;
        }

        public Interactive GetInteractive(int scaledTileSize, Vector2 centerOffset, float upRad=0, float downRad=0, float leftRad=0, float rightRad=0)
        {
            var x = centerOffset.X / scaledTileSize;
            var y = centerOffset.Y / scaledTileSize;
            for (var i = x - leftRad; i <= x + rightRad; i+=0.25f)
            {
                if (i < 0 || i >= X) continue;
                for (var j = y - upRad; j <= y + downRad; j+=0.25f)
                {
                    if (j < 0 || j >= Y) continue;
                    var obj = _interactive[MathD.RoundDown(j)][MathD.RoundDown(i)];
                    if (obj != null) return obj;
                }
            }

            return null;
        }

        public Interactive GetInteractive(int x, int y)
        {
            return _interactive[y][x];
        }
    }
}
