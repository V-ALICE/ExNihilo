using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Util.Graphics
{
    public static class TextureLibrary
    {
        public static Dictionary<string, AnimatableTexture> TextureLookUp;

        public static void LoadLibrary(GraphicsDevice graphics, ContentManager content, params string[] infoFiles)
        {
            TextureLookUp = new Dictionary<string, AnimatableTexture> {{"null", new AnimatableTexture(new Texture2D(graphics, 1, 1))}};

            foreach (var file in infoFiles)
            {
                var fileName = Environment.CurrentDirectory + "/Content/SheetInfo/" + file;
                if (!File.Exists(fileName)) continue;

                var lines = File.ReadAllLines(fileName);
                if (lines.Length == 0) continue;

                Texture2D sheet;
                try
                {
                    sheet = content.Load<Texture2D>(lines[0].Trim());
                }
                catch (Exception)
                {
                    continue;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    var segments = lines[i].Split(' ', '\t');
                    var trueSegments = segments.Select(segment => segment.Trim()).Where(trim => trim.Length > 0).ToList();
                    if (trueSegments.Count < 5) continue;

                    var name = trueSegments[0];
                    if (TextureLookUp.ContainsKey(name)) continue;
                    if (!int.TryParse(trueSegments[1], out int x)) continue;
                    if (!int.TryParse(trueSegments[2], out int y)) continue;
                    if (!int.TryParse(trueSegments[3], out int width)) continue;
                    if (!int.TryParse(trueSegments[4], out int height)) continue;
                    if (trueSegments.Count == 7)
                    {
                        if (!int.TryParse(trueSegments[5], out int frames)) continue;
                        if (!int.TryParse(trueSegments[5], out int fps)) continue;
                        TextureLookUp.Add(name, new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(x, y, width, height)), frames, fps));
                    }
                    else TextureLookUp.Add(name, new AnimatableTexture(TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(x, y, width, height))));
                }

                sheet.Dispose();
            }
        }
    }
}
