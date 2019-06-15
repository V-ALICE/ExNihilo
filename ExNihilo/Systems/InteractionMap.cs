using System.IO;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems
{
    public class InteractionMap
    {
        public enum Type
        {
            None, Air, Ground, Wall
        }

        private readonly int _x, _y;
        private readonly Type[][] _map;
        private readonly Interactive[][] _interactive;

        public InteractionMap(string fileName)
        {
            var map = File.ReadAllLines("Content/Resources/" + fileName);
            if (map.Length == 0) return;
            _x = map[0].Length;
            _y = map.Length;
            _map = new Type[_y][];
            _interactive = new Interactive[_y][];
            for (int i = 0; i < _y; i++)
            {
                _map[i] = new Type[_x];
                _interactive[i] = new Interactive[_x];
                for (int j = 0; j < _x; j++)
                {
                    switch (map[i][j])
                    {
                        case 'A':
                            _map[i][j] = Type.Air;
                            break;
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
                    _interactive[i][j] = null;
                }
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
                    if (_map[i][j] != Type.Ground) return true;
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
