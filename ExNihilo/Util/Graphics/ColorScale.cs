using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util.Graphics
{
    public class ColorScale
    {
        private static readonly Dictionary<string, ColorScale> _globalScaleMap = new Dictionary<string, ColorScale>();
        public static ColorScale White = new ColorScale(Color.White);
        public static ColorScale Black = new ColorScale(Color.Black);
        public static ColorScale Grey = new ColorScale(Color.Gray);
        public static ColorScale Ghost = new ColorScale(new Color(160, 160, 160, 128));

        public static void LoadColors(string file)
        {
            var fileName = Environment.CurrentDirectory + "/Content/Resources/" + file;
            if (!File.Exists(fileName)) return;
            var lines = File.ReadAllLines(fileName);

            foreach (var line in lines)
            {
                if (line.Length == 0) continue;
                try
                {
                    var set = line.Split(' ');
                    if (set.Length < 5 || (set.Length-2) % 3 != 0) throw new Exception();
                    var all = new List<Color>();
                    for (int i = 2; i < set.Length; i += 3)
                    {
                        all.Add(new Color(int.Parse(set[i]), int.Parse(set[i + 1]), int.Parse(set[i + 2])));
                    }

                    var first = all[0];
                    all.RemoveAt(0);
                    AddToGlobal(set[0], new ColorScale(float.Parse(set[1]), false, first, all.ToArray()));
                }
                catch (Exception)
                {
                    GameContainer.Console.ForceMessage("<warning>", "Ignoring malformed color line \"" + line + "\"", Color.DarkOrange, Color.White);
                }
            }
        }

        private readonly bool _random, _oneWay;
        private readonly byte _upper, _lower;
        private readonly List<Color> _colors;
        private static readonly Random _rand = new Random();
        private readonly int _timerID = -1;

        public ColorScale(float lerpTime, byte lowerBound, byte upperBound)
        {
            //random rainbow
            _random = true;
            _upper = upperBound;
            _lower = lowerBound;
            _colors = new List<Color>
            {
                new Color(_upper, _upper, _upper),
                new Color((byte) _rand.Next(_lower, _upper), (byte) _rand.Next(_lower, _upper), (byte) _rand.Next(_lower, _upper))
            };
            _timerID = UniversalTime.NewTimer(false, lerpTime);
            UniversalTime.TurnOnTimer(_timerID);
        }
        public ColorScale(float lerpTimePerColor, bool oneWay, Color firstColor, params Color[] colors)
        {
            //set of colors
            _oneWay = oneWay; //Go through all colors and then stop
            _colors = new List<Color> {firstColor};
            foreach (var color in colors) _colors.Add(color);
            _timerID = UniversalTime.NewTimer(false, lerpTimePerColor);
            UniversalTime.TurnOnTimer(_timerID);
        }
        public ColorScale(Color color)
        {
            //single color
            _colors = new List<Color> {color};
        }

        public static implicit operator ColorScale(Color c)
        {
            return new ColorScale(c);
        }

        public static implicit operator Color(ColorScale c)
        {
            return c?.Get() ?? Color.Black;
        }

        public static implicit operator ColorScale[](ColorScale c)
        {
            return new[] {c};
        }

        ~ColorScale()
        {
            if (_timerID >= 0) UniversalTime.SellTimer(_timerID);
        }

        public static ColorScale GetFromGlobal(string name)
        {
            var c = _globalScaleMap.TryGetValue(name, out var color);
            if (c) return color;
            GameContainer.Console.ForceMessage("<error>", "Trying to load nonexistent color \"" + name + "\"", Color.DarkRed, Color.White);
            return null;
        }

        public static void AddToGlobal(string name, ColorScale scale)
        {
            if (_globalScaleMap.ContainsKey(name)) _globalScaleMap[name] = scale;
            else _globalScaleMap.Add(name, scale);
        }

        public static void UpdateGlobalScales()
        {
            foreach (var scale in _globalScaleMap) scale.Value.Update();
        }

        public void Update()
        {
            if (_colors.Count == 1) return;
            if (UniversalTime.GetNumberOfFires(_timerID) > 0)
            {
                if (_random)
                {
                    //random
                    _colors.RemoveAt(0);
                    _colors.Add(new Color((byte) _rand.Next(_lower, _upper), (byte) _rand.Next(_lower, _upper), (byte) _rand.Next(_lower, _upper)));
                }
                else if (_oneWay)
                {
                    //non-looping non-random
                    _colors.RemoveAt(0);
                }
                else
                {
                    //looping non-random
                    _colors.Add(_colors[0]);
                    _colors.RemoveAt(0);
                }
            }           
        }

        public Color Get()
        {
            if (_colors.Count == 1) return _colors[0];
            return Color.Lerp(_colors[0], _colors[1], (float)UniversalTime.GetPercentageDone(_timerID));
        }
    }
}