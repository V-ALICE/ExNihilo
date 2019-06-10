using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ExNihilo.Util;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;

namespace ExNihilo.UI.Bases
{
    public static class UILibrary
    {
        public static Dictionary<string, AnimatableTexture> TextureLookUp;
        public static Dictionary<string, byte[]> TextureAlphaLookUp;
        public static ScaleRuleSet DefaultScaleRuleSet, ReducedScaleRuleSet, UnscaledScaleRuleSet, HalfScaleRuleSet, DoubleScaleRuleSet;

        public static void LoadRuleSets()
        {
            DefaultScaleRuleSet = new ScaleRuleSet();
            DefaultScaleRuleSet.AddRules
            (
                new ScaleRule(1, 1400, 1000),
                new ScaleRule(2, 2100, 1500),
                new ScaleRule(3, 2800, 2000),
                new ScaleRule(4, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            ReducedScaleRuleSet = new ScaleRuleSet();
            ReducedScaleRuleSet.AddRules
            (
                new ScaleRule(1, 2100, 1500),
                new ScaleRule(2, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            UnscaledScaleRuleSet = new ScaleRuleSet();
            UnscaledScaleRuleSet.AddRules
            (
                new ScaleRule(1, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            HalfScaleRuleSet = new ScaleRuleSet();
            HalfScaleRuleSet.AddRules
            (
                new ScaleRule(0.5f, 1400, 1000),
                new ScaleRule(1, 2100, 1500),
                new ScaleRule(1.5f, 2800, 2000),
                new ScaleRule(2, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            DoubleScaleRuleSet = new ScaleRuleSet();
            DoubleScaleRuleSet.AddRules
            (
                new ScaleRule(2, 1400, 1000),
                new ScaleRule(4, 2100, 1500),
                new ScaleRule(6, 2800, 2000),
                new ScaleRule(8, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );
        }

        public static void LoadLibrary(GraphicsDevice graphics, ContentManager content, params string[] infoFiles)
        {
            TextureLookUp = new Dictionary<string, AnimatableTexture> {{"null", new Texture2D(graphics, 1, 1)}};

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
                    else TextureLookUp.Add(name, TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(x, y, width, height)));
                }

                sheet.Dispose();
            }

            TextureAlphaLookUp = new Dictionary<string, byte[]>();
            foreach (var texture in TextureLookUp)
            {
                TextureAlphaLookUp.Add(texture.Key, texture.Value.GetAlphaMask());
            }
        }

    }
}
