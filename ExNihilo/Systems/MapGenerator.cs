
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems
{
    public static class MapGenerator
    {
        public enum Type
        {
            Default
        }

        public static List<List<TypeMatrix.Type>> Get(Type type=Type.Default)
        {
            var map = new List<List<TypeMatrix.Type>>();

            return map;
        }

        public static Texture2D StitchMap(GraphicsDevice graphics, TypeMatrix set, int tileSize)
        {
            var texture = new Texture2D(graphics, tileSize*set.X, tileSize*set.Y);

            return texture;
        }
    }
}
