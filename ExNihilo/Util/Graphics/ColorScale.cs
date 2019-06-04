using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util.Graphics
{
    public class ColorScale
    {
        private readonly bool _random, _singular;
        private bool _halted;
        private readonly byte _upper, _lower;
        private Color _first, _next;
        private static readonly Random _rand = new Random();
        private readonly int _timerID;

        public ColorScale(float lerpTime, byte lowerBound, byte upperBound)
        {//rainbow sort of
            _random = true;
            _upper = upperBound;
            _lower = lowerBound;
            _first = new Color(_upper, _upper, _upper);
            _next = new Color((byte)_rand.Next(_lower, _upper), (byte)_rand.Next(_lower, _upper), (byte)_rand.Next(_lower, _upper));
            _timerID = UniversalTime.NewTimer(false, lerpTime);
            UniversalTime.TurnOnTimer(_timerID);
        }
        public ColorScale(float lerpTime, Color lowerBound, Color upperBound, bool oneWay=false)
        {//between two colors
            _random = false;
            _singular = oneWay; //Go from lower to upper bound and then stop
            _first = lowerBound;
            _next = upperBound;
            _timerID = UniversalTime.NewTimer(false, lerpTime);
            UniversalTime.TurnOnTimer(_timerID);
        }
        ~ColorScale()
        {
            UniversalTime.SellTimer(_timerID);
        }

        public void Update()
        {
            if (UniversalTime.GetNumberOfFires(_timerID) > 0)
            {
                if (_random)
                {
                    _first = _next;
                    _next = new Color((byte) _rand.Next(_lower, _upper), (byte) _rand.Next(_lower, _upper), (byte) _rand.Next(_lower, _upper));
                }
                else if (!_singular)
                {
                    var tmp = _next.ToVector3();
                    _next = _first;
                    _first = new Color(tmp);
                }
                else _halted = true;
            }           
        }

        public Color Get()
        {
            return _halted ? _next : Color.Lerp(_first, _next, (float)UniversalTime.GetPercentageDone(_timerID));
        }
    }
}