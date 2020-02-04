using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle; 
using FormRectangle = System.Drawing.Rectangle; 
using Color = Microsoft.Xna.Framework.Color; 

namespace ExNihilo.Util.Graphics
{
    public static class TextureUtilities
    {
        public enum PositionType
        {
            TopLeft, TopRight,
            BottomLeft, BottomRight,
            CenterTop, CenterBottom,
            CenterLeft, CenterRight,
            Center
        }

        public static Vector2 GetOffset(PositionType anchorType, AnimatableTexture texture)
        {
            return GetOffset(anchorType, new Coordinate(texture.Width, texture.Height));
        }

        public static Vector2 GetOffset(PositionType anchorType, Coordinate pixelSize)
        {
            Vector2 textureOffsetToOrigin;
            switch (anchorType)
            {
                case PositionType.TopLeft:
                    textureOffsetToOrigin = new Vector2(0, 0);
                    break;
                case PositionType.TopRight:
                    textureOffsetToOrigin = new Vector2(pixelSize.X, 0);
                    break;
                case PositionType.BottomLeft:
                    textureOffsetToOrigin = new Vector2(0, pixelSize.Y);
                    break;
                case PositionType.BottomRight:
                    textureOffsetToOrigin = new Vector2(pixelSize.X, pixelSize.Y);
                    break;
                case PositionType.CenterTop:
                    textureOffsetToOrigin = new Vector2(pixelSize.X / 2f, 0);
                    break;
                case PositionType.CenterBottom:
                    textureOffsetToOrigin = new Vector2(pixelSize.X / 2f, pixelSize.Y);
                    break;
                case PositionType.CenterLeft:
                    textureOffsetToOrigin = new Vector2(0, pixelSize.Y / 2f);
                    break;
                case PositionType.CenterRight:
                    textureOffsetToOrigin = new Vector2(pixelSize.X, pixelSize.Y / 2f);
                    break;
                case PositionType.Center:
                    textureOffsetToOrigin = new Vector2(pixelSize.X / 2f, pixelSize.Y / 2f);
                    break;
                default:
                    textureOffsetToOrigin = new Vector2();
                    break;
            }

            return textureOffsetToOrigin;
        }

        public static Texture2D Extend3FramesTo4(GraphicsDevice graphics, Texture2D a)
        {
            //taken directly from the previous project since it works
            int oldW = a.Width, sing = oldW / 3, newW = sing * 4;
            var rect = new Rectangle(0, 0, newW, a.Height);
            var texture = new Texture2D(graphics, rect.Width, rect.Height);
            var data = new Color[rect.Width * rect.Height];
            var adata = new Color[a.Width * a.Height];
            a.GetData(0, new Rectangle(0, 0, a.Width, a.Height), adata, 0, adata.Length);
            for (var i = 0; i < data.Length; i++)
            {
                if (i % newW < oldW) data[i] = adata[oldW * (i / newW) + i % newW];
                else data[i] = adata[oldW * (i / newW) + sing + (i % newW - oldW)];
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CombineTextures(GraphicsDevice graphics, params Texture2D[] sheets)
        {
            //first sheet given is lowest layer
            if (sheets.Length == 0) return null;
            if (sheets.Length == 1) return sheets[0];

            var texture = new Texture2D(graphics, sheets[0].Width, sheets[1].Height);
            var rect = new Rectangle(0, 0, texture.Width, texture.Height);
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(0, rect, data, 0, data.Length);
            foreach (var sheet in sheets)
            {
                var topData = new Color[rect.Width * rect.Height];
                if (topData.Length != data.Length) continue;

                sheet.GetData(0, rect, topData, 0, topData.Length);
                for (var j = 0; j < topData.Length; j++)
                {
                    if (topData[j].A != 0) data[j] = topData[j];
                }
                texture.SetData(data);
            }

            return texture;
        }

        public static Texture2D GetSubTexture(GraphicsDevice device, Texture2D sheet, Rectangle rect)
        {
            //while (GAME.FormTouched) { Thread.Sleep(100); }
            if (device == null) return null;
            var texture = new Texture2D(device, rect.Width, rect.Height);
            if (rect.Width == 0 || rect.Height == 0) return texture;
            texture.SetData(GetColorData(sheet, rect));
            return texture;
        }

        private static Color[] GetColorData(Texture2D sheet, Rectangle rect)
        {
            //while (GAME.FormTouched) { Thread.Sleep(100); }
            try
            {
                var data = new Color[rect.Width * rect.Height];
                sheet.GetData(0, rect, data, 0, data.Length);
                return data;
            }
            catch (NullReferenceException)
            {
            }
            return new Color[0];
        }

        public static void SetSubTexture(Texture2D main, Texture2D sub, int x, int y)
        {
            Color[] subData = new Color[sub.Width*sub.Height];
            sub.GetData(subData);
            main.SetData(0, new Rectangle(x, y, sub.Width, sub.Height), subData, 0, subData.Length);
        }

        public static Texture2D CreateSingleColorTexture(GraphicsDevice device, int width, int height, Color color)
        {
            var texture = new Texture2D(device, width, height);
            var data = new Color[width * height];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            texture.SetData(data);
            return texture;
        }

        public static void WriteTextureToPNG(Texture2D texture, string filename, string directory = "")
        {
            if (directory.Length > 0)
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                filename = directory + "/" + filename;
            }
            var textureData = new byte[4 * texture.Width * texture.Height];
            var bmp = new Bitmap(texture.Width, texture.Height, PixelFormat.Format32bppArgb);

            texture.GetData(textureData);
            for (var i = 0; i < textureData.Length; i += 4)
            {
                var blue = textureData[i];
                textureData[i] = textureData[i + 2];
                textureData[i + 2] = blue;
            }

            var rect = new FormRectangle(0, 0, texture.Width, texture.Height);
            var bitmapData = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            var safePtr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bmp.UnlockBits(bitmapData);

            bmp.Save(filename, ImageFormat.Png);
        }
    }
}
