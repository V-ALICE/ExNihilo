using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle; 
using FormRectangle = System.Drawing.Rectangle; 
using Color = Microsoft.Xna.Framework.Color; 

namespace ExNihilo.Util.Graphics
{
    public static class TextureUtilities
    {

        public static Texture2D GetSubTexture(GraphicsDevice device, Texture2D sheet, int[] coords)
        {
            return GetSubTexture(device, sheet, new Rectangle(coords[0], coords[1], coords[2], coords[3]));
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
            catch (NullReferenceException e)
            {
                ExceptionCheck.Fail(e.Message);
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
