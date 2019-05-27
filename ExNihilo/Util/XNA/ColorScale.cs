using System;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util.XNA
{
    public class ColorScale
    {
        private readonly bool random, singular;
        private bool halted;
        private readonly byte upper, lower;
        private Color first, next;
        private readonly Random rand;
        private readonly int timerID;

        public ColorScale(float lerpTime, byte lowerBound, byte upperBound)
        {//rainbow sort of
            random = true;
            upper = upperBound;
            lower = lowerBound;
            first = new Color(upper, upper, upper);
            rand = new Random();
            next = new Color((byte)rand.Next(lower, upper), (byte)rand.Next(lower, upper), (byte)rand.Next(lower, upper));
            timerID = UniversalTime.RequestTimer(false, lerpTime);
            UniversalTime.TurnOnTimer(timerID);
        }
        public ColorScale(float lerpTime, Color lowerBound, Color upperBound, bool oneWay=false)
        {//between two colors
            random = false;
            singular = oneWay; //Go from lower to upper bound and then stop
            first = lowerBound;
            next = upperBound;
            timerID = UniversalTime.RequestTimer(false, lerpTime);
            UniversalTime.TurnOnTimer(timerID);
        }
        ~ColorScale()
        {
            UniversalTime.SellTimer(timerID);
        }

        public void Update()
        {
            if (UniversalTime.RequestNumberOfFires(timerID) > 0)
            {
                if (random)
                {
                    first = next;
                    next = new Color((byte) rand.Next(lower, upper), (byte) rand.Next(lower, upper), (byte) rand.Next(lower, upper));
                }
                else if (!singular)
                {
                    var tmp = next.ToVector3();
                    next = first;
                    first = new Color(tmp);
                }
                else halted = true;
            }           
        }

        public Color Get()
        {
            return halted ? next : Color.Lerp(first, next, (float)UniversalTime.RequestPercentageDone(timerID));
        }
    }
}