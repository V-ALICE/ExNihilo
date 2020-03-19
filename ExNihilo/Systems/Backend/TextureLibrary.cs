using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Backend
{
    public static class TextureLibrary
    {
        public static ScaleRuleSet DefaultScaleRuleSet, ReducedDefaultScaleRuleSet, HalfScaleRuleSet, QuarterScaleRuleSet, 
                                   ThreeQuarterScaleRuleSet, x1d25ScaleRuleSet, x1d5ScaleRuleSet, DoubleScaleRuleSet, 
                                   QuadScaleRuleSet, x5ScaleRuleSet;
        private static Dictionary<string, Dictionary<string, Texture2D>> _UILookUp;
        private static Dictionary<string, Dictionary<string, Texture2D>> _textureLookUp;
        private static Dictionary<string, Dictionary<string, Texture2D>> _iconLookUp;
        private static List<Tuple<string, List<Texture2D>>> _characterLookUp;
        private static Texture2D _null;

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
                        default:
                            return _textureLookUp[tmp[1]][tmp[2]];
                    }
                }
                catch (KeyNotFoundException)
                {
                    SystemConsole.ForceMessage("<warning>", "No such texture \"" + fullPath + "\"", Color.DarkOrange, Color.White);
                    return _null;
                }
            }

            return _null;
        }

        public static Texture2D CharLookup(string name, int index)
        {
            if (name == "null") return _null;
            var set = _characterLookUp.FirstOrDefault(g => g.Item1 == name);
            if (set != null && set.Item2.Count > index) return set.Item2[index];
            SystemConsole.ForceMessage("<error>", "No such char with name " + name + " and charIndex " + index, Color.DarkRed, Color.White);
            return _null;
        }
        public static Texture2D CharLookup(int nameIndex, int charIndex)
        {
            if (nameIndex == -1) return _null;
            try
            {
                return _characterLookUp[nameIndex].Item2[charIndex];
            }
            catch (IndexOutOfRangeException)
            {
                SystemConsole.ForceMessage("<error>", "No such char with name_index " + nameIndex + " and charIndex " + charIndex, Color.DarkRed, Color.White);
                return _null;
            }
        }
        public static string GetCharSetByIndex(int index)
        {
            try
            {
                return _characterLookUp[index].Item1;
            }
            catch (IndexOutOfRangeException)
            {
                SystemConsole.ForceMessage("<error>", "No such set with name_index " + index, Color.DarkRed, Color.White);
                return "null";
            }
        }
        public static int GetCharSetIndexByName(string name)
        {
            for (int i=0;i<_characterLookUp.Count;i++)
            {
                if (_characterLookUp[i].Item1 == name) return i;
            }
            SystemConsole.ForceMessage("<error>", "No such set with name " + name, Color.DarkRed, Color.White);
            return -1;
        }
        public static void CorrectCharIndicies(ref int set, ref int index)
        {
            if (set < 0) set = _characterLookUp.Count - 1;
            else if (set >= _characterLookUp.Count) set = 0;
            if (index < 0) index = _characterLookUp[set].Item2.Count - 1;
            else if (index >= _characterLookUp[set].Item2.Count) index = 0;
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

            ThreeQuarterScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(0.75f, 1400, 1000),
                new ScaleRule(1.5f, 2100, 1500),
                new ScaleRule(2.25f, 2800, 2000),
                new ScaleRule(3f, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            x1d25ScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(1.25f, 1400, 1000),
                new ScaleRule(2.5f, 2100, 1500),
                new ScaleRule(3.75f, 2800, 2000),
                new ScaleRule(5f, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            x1d5ScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(1.5f, 1400, 1000),
                new ScaleRule(3f, 2100, 1500),
                new ScaleRule(4.5f, 2800, 2000),
                new ScaleRule(6f, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            ReducedDefaultScaleRuleSet = new ScaleRuleSet
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

            QuarterScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(0.25f, 1400, 1000),
                new ScaleRule(0.5f, 2100, 1500),
                new ScaleRule(0.75f, 2800, 2000),
                new ScaleRule(1, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            DoubleScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(2, 1400, 1000),
                new ScaleRule(4, 2100, 1500),
                new ScaleRule(6, 2800, 2000),
                new ScaleRule(8, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            QuadScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(4, 1400, 1000),
                new ScaleRule(8, 2100, 1500),
                new ScaleRule(12, 2800, 2000),
                new ScaleRule(16, ScaleRule.MAX_X, ScaleRule.MAX_Y)
            );

            x5ScaleRuleSet = new ScaleRuleSet
            (
                new ScaleRule(5, 1400, 1000),
                new ScaleRule(10, 2100, 1500),
                new ScaleRule(15, 2800, 2000),
                new ScaleRule(20, ScaleRule.MAX_X, ScaleRule.MAX_Y)
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

        public static void LoadCharacterLibrary(GraphicsDevice graphics, ContentManager content, int width, int height)
        {
            if (_null is null) _null = new Texture2D(graphics, 1, 1);

            _characterLookUp = new List<Tuple<string, List<Texture2D>>>();
            var fileSet = Directory.GetFiles(Environment.CurrentDirectory + "/Content/CharSets/");
            foreach (var file in fileSet)
            {
                if (!file.EndsWith(".png")) continue;

                var setName = Path.GetFileName(file);
                setName = setName.Substring(0, setName.Length - 4);
                var list = new List<Texture2D>();
                _characterLookUp.Add(Tuple.Create(setName, list));

                var stream = new FileStream(file, FileMode.Open);
                var tex = Texture2D.FromStream(graphics, stream);
                stream.Close();

                for (int y = 0; y <= tex.Height - height; y += height)
                {
                    for (int x = 0; x <= tex.Width - width; x += width)
                    {
                        var sub = TextureUtilities.GetSubTexture(graphics, tex, new Rectangle(x, y, width, height));
                        var expandedSub = TextureUtilities.Extend3FramesTo4(graphics, sub);
                        list.Add(expandedSub);
                        sub.Dispose();
                    }
                }
                tex.Dispose();
            }
        }
    }
}
