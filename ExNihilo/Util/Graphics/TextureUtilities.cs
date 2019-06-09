using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
            Center,
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
                    textureOffsetToOrigin = new Vector2(pixelSize.X / 2, 0);
                    break;
                case PositionType.CenterBottom:
                    textureOffsetToOrigin = new Vector2(pixelSize.X / 2, pixelSize.Y);
                    break;
                case PositionType.CenterLeft:
                    textureOffsetToOrigin = new Vector2(0, pixelSize.Y / 2);
                    break;
                case PositionType.CenterRight:
                    textureOffsetToOrigin = new Vector2(pixelSize.X, pixelSize.Y / 2);
                    break;
                case PositionType.Center:
                    textureOffsetToOrigin = new Vector2(pixelSize.X / 2, pixelSize.Y / 2);
                    break;
                default:
                    textureOffsetToOrigin = new Vector2();
                    break;
            }

            return textureOffsetToOrigin;
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
