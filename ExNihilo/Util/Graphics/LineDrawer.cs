using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Util.Graphics
{
    public static class LineDrawer
    {
        private const float Deg0 = -Deg180;
        private const float Deg90 = 0;
        private const float Deg180 = (float) (Math.PI / 2);
        private const float Deg270 = (float) Math.PI;

        private static Texture2D _texture;
        private static Texture2D GetTexture(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _texture.SetData(new[] { Color.White });
            }

            return _texture;
        }

        public static void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }

        public static void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }

        public static void DrawSquare(SpriteBatch spriteBatch, Vector2 point, int X, int Y, Color color, float thickness = 1f)
        {
            DrawLine(spriteBatch, point, X, Deg90, color, thickness);
            DrawLine(spriteBatch, point, Y, Deg180, color, thickness);
            var opposite = point + new Vector2(X, Y);
            DrawLine(spriteBatch, opposite, X, Deg270, color, thickness);
            DrawLine(spriteBatch, opposite, Y, Deg0, color, thickness);
        }
    }
}
