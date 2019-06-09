using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util.Graphics
{
    public class ColorScale
    {
        private static readonly Dictionary<string, ColorScale> _globalScaleMap = new Dictionary<string, ColorScale>();
        public static ColorScale White = new ColorScale(Color.White);
        public static ColorScale Black = new ColorScale(Color.Black);

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
            return c.Get();
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
            return _globalScaleMap.ContainsKey(name) ? _globalScaleMap[name] : null;
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