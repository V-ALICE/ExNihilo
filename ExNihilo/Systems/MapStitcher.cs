
using System;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tile = ExNihilo.Systems.TypeMatrix.Type;

namespace ExNihilo.Systems
{
    public static class MapStitcher
    {
        public static Texture2D StitchMap(GraphicsDevice graphics, TypeMatrix set, int tileSize)
        {
            var texture = new Texture2D(graphics, tileSize * set.X, tileSize * set.Y);
            var wall = TextureUtilities.CreateSingleColorTexture(graphics, tileSize, tileSize, Color.DarkRed);
            var floor = TextureUtilities.CreateSingleColorTexture(graphics, tileSize, tileSize, Color.ForestGreen);
            for (int i = 0; i < set.Y; i++)
            {
                for (int j = 0; j < set.X; j++)
                {
                    var tile = set.Get(j, i);
                    if (tile == Tile.Ground) TextureUtilities.SetSubTexture(texture, floor, j * tileSize, i * tileSize);
                    else if (tile == Tile.Wall) TextureUtilities.SetSubTexture(texture, wall, j * tileSize, i * tileSize);
                }
            }
            //TextureUtilities.WriteTextureToPNG(texture, DateTime.Now.Second+"-"+ DateTime.Now.Millisecond+".png", "C:/Users/Alice/Desktop/garbage");
            return texture;
        }
    }
}
