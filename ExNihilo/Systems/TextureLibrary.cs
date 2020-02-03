using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems
{
    public static class TextureLibrary
    {
        public static ScaleRuleSet DefaultScaleRuleSet, ReducedScaleRuleSet, HalfScaleRuleSet, DoubleScaleRuleSet, GiantScaleRuleSet;
        private static Dictionary<string, Dictionary<string, Texture2D>> _UILookUp;
        private static Dictionary<string, Dictionary<string, Texture2D>> _textureLookUp;
        private static Dictionary<string, Dictionary<string, Texture2D>> _iconLookUp;
        private static Dictionary<string, Dictionary<string, Texture2D>> _characterLookUp;
        private static Texture2D _null;
        //private static Dictionary<string, byte[]> _UIAlphaLookUp;

        public static Texture2D Lookup(string fullPath)
        {
            var tmp = fullPath.Split('/');
            if (tmp.Length == 3)
            {
                try
                {
                    switch (tmp[0])
                    {
                        case "UI":
                            return _UILookUp[tmp[1]][tmp[2]];
                        case "Icon":
                            return _iconLookUp[tmp[1]][tmp[2]];
                        case "Char":
                            return _characterLookUp[tmp[1]][tmp[2]];
                        default:
                            return _textureLookUp[tmp[1]][tmp[2]];
                    }
                }
                catch (KeyNotFoundException)
                {
                    return _null;
                }
            }

            return _null;
        }

        public static void LoadRuleSets()
        {
            // 700 x 500 base only works for fullscreen
            DefaultScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(1, 1400, 1000),
                new ScaleRule(2, 2100, 1500),
                new ScaleRule(3, 2800, 2000),
                new ScaleRule(4, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            ReducedScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(1, 2100, 1500),
                new ScaleRule(2, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            HalfScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(0.5f, 1400, 1000),
                new ScaleRule(1, 2100, 1500),
                new ScaleRule(1.5f, 2800, 2000),
                new ScaleRule(2, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            DoubleScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(2, 1400, 1000),
                new ScaleRule(4, 2100, 1500),
                new ScaleRule(6, 2800, 2000),
                new ScaleRule(8, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            GiantScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(4, 1400, 1000),
                new ScaleRule(8, 2100, 1500),
                new ScaleRule(12, 2800, 2000),
                new ScaleRule(16, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

        }

        private static void LoadLibrary(GraphicsDevice graphics, ContentManager content, Dictionary<string, Dictionary<string, Texture2D>> d, params string[] infoFiles)
        {
            foreach (var file in infoFiles)
            {
                var fileName = Environment.CurrentDirectory + "/Content/Resources/" + file;
                if (!File.Exists(fileName)) continue;

                var lines = File.ReadAllLines(fileName);
                if (lines.Length == 0) continue;

                Texture2D sheet = null;
                foreach (var t in lines)
                {
                    var segments = t.Split(' ', '\t');
                    var trueSegments = segments.Select(segment => segment.Trim()).Where(trim => trim.Length > 0).ToList();
                    if (trueSegments.Count == 1)
                    {
                        //new file
                        sheet?.Dispose();
                        try
                        {
                            sheet = content.Load<Texture2D>(t.Trim());
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        continue;
                    }
                    if (trueSegments.Count != 5 || sheet is null) continue;

                    var name = trueSegments[0].Split('/');
                    if (!d.ContainsKey(name[0])) d.Add(name[0], new Dictionary<string, Texture2D>());
                    var sub = d[name[0]];
                    
                    if (sub.ContainsKey(name[1])) continue;
                    if (!int.TryParse(trueSegments[1], out int x)) continue;
                    if (!int.TryParse(trueSegments[2], out int y)) continue;
                    if (!int.TryParse(trueSegments[3], out int width)) continue;
                    if (!int.TryParse(trueSegments[4], out int height)) continue;
                    sub.Add(name[1], TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(x, y, width, height)));
                }
            }
        }

        public static void LoadUILibrary(GraphicsDevice graphics, ContentManager content, params string[] infoFiles)
        {
            if (_null is null) _null = new Texture2D(graphics, 1, 1);
            _UILookUp = new Dictionary<string, Dictionary<string, Texture2D>>();
            LoadLibrary(graphics, content, _UILookUp, infoFiles);
        }

        public static void LoadTextureLibrary(GraphicsDevice graphics, ContentManager content, params string[] infoFiles)
        {
            if (_null is null) _null = new Texture2D(graphics, 1, 1);
            _textureLookUp = new Dictionary<string, Dictionary<string, Texture2D>>();
            LoadLibrary(graphics, content, _textureLookUp, infoFiles);
        }

        public static void LoadIconLibrary(GraphicsDevice graphics, ContentManager content, params string[] infoFiles)
        {
            if (_null is null) _null = new Texture2D(graphics, 1, 1);
            _iconLookUp = new Dictionary<string, Dictionary<string, Texture2D>>();
            LoadLibrary(graphics, content, _iconLookUp, infoFiles);
        }

        public static void LoadCharacterLibrary(GraphicsDevice graphics, ContentManager content, params string[] infoFiles)
        {
            if (_null is null) _null = new Texture2D(graphics, 1, 1);
            _characterLookUp = new Dictionary<string, Dictionary<string, Texture2D>>();
            LoadLibrary(graphics, content, _characterLookUp, infoFiles);

            //dumb but probably beats having to make 400 extra lines in the input
            var colorSets = _characterLookUp["hair"];
            _characterLookUp["hair"] = new Dictionary<string, Texture2D>();
            foreach (var c in colorSets)
            {
                var sheet = c.Value;
                for (int i = 0; i < 10; i++)
                {
                    var hairSheet = TextureUtilities.GetSubTexture(graphics, sheet, new Rectangle(i * 96, 0, 96, 144));
                    var extendedHairSheet = TextureUtilities.Extend3FramesTo4(graphics, hairSheet);
                    _characterLookUp["hair"].Add(c.Key + "-" + (i+1), extendedHairSheet);
                }
            }
        }
    }
}
