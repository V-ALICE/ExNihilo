using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using ExNihilo.Entity;
using ExNihilo.Systems.Backend;
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

        public static Coordinate GetOffset(PositionType anchorType, AnimatableTexture texture)
        {
            return GetOffset(anchorType, new Coordinate(texture.Width, texture.Height));
        }

        public static Coordinate GetOffset(PositionType anchorType, Coordinate pixelSize)
        {
            switch (anchorType)
            {
                case PositionType.TopLeft:
                    return new Coordinate(0, 0);
                case PositionType.TopRight:
                    return new Coordinate(pixelSize.X, 0);
                case PositionType.BottomLeft:
                    return new Coordinate(0, pixelSize.Y);
                case PositionType.BottomRight:
                    return new Coordinate(pixelSize.X, pixelSize.Y);
                case PositionType.CenterTop:
                    return new Coordinate(pixelSize.X / 2, 0);
                case PositionType.CenterBottom:
                    return new Coordinate(pixelSize.X / 2, pixelSize.Y);
                case PositionType.CenterLeft:
                    return new Coordinate(0, pixelSize.Y / 2);
                case PositionType.CenterRight:
                    return new Coordinate(pixelSize.X, pixelSize.Y / 2);
                case PositionType.Center:
                    return new Coordinate(pixelSize.X / 2, pixelSize.Y / 2);
                default:
                    return new Coordinate();
            }
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
            while (GameContainer.FormTouched) { Thread.Sleep(10); }
            if (device == null) return null;
            if (rect.Width == 0 || rect.Height == 0) return null;
            var texture = new Texture2D(device, rect.Width, rect.Height);
            texture.SetData(GetColorData(sheet, rect));
            return texture;
        }

        private static Color[] GetColorData(Texture2D sheet, Rectangle rect)
        {
            while (GameContainer.FormTouched) { Thread.Sleep(10); }
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
            if (main is null || sub is null) return;
            if (x < 0 || y < 0 || x + sub.Width > main.Width || y + sub.Height > main.Height) return;
            while (GameContainer.FormTouched) { Thread.Sleep(10); }
            //try
            {
                Color[] subData = new Color[sub.Width * sub.Height];
                sub.GetData(subData);
                main.SetData(0, new Rectangle(x, y, sub.Width, sub.Height), subData, 0, subData.Length);
            }
            //catch (Exception)
            {
            }

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
            if (texture is null) return;

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

        public static Texture2D GetPlayerTexture(GraphicsDevice graphics, int[] charSet)
        {
            var bodySheet = TextureLibrary.Lookup("Char/base/" + (charSet[0] + 1));
            var hairSheet = TextureLibrary.Lookup("Char/hair/" + (charSet[1] + 1) + "-" + (charSet[3] + 1));
            var clothSheet = TextureLibrary.Lookup("Char/cloth/" + (charSet[2] + 1));
            return CombineTextures(graphics, bodySheet, clothSheet, hairSheet);
        }
    }
}
