using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    public static class UniversalTime
    {
        //Timers that are being tracked are added to the bus. All active timers on the bus are updated per time step
        private static readonly List<Timer> _timeBus = new List<Timer>();
        //Timers that are no longer needed by their Geter have their seats on the bus sold back for the next Geter
        private static readonly List<int> _timersForSale = new List<int>();

        public static int NewTimer(bool systemTimer, double maxTimeSec=-1)
        {
            var newTimer = new Timer(maxTimeSec, systemTimer);
            if (_timersForSale.Count > 0)
            {
                var bought = _timersForSale[0];
                _timersForSale.RemoveAt(0);
                _timeBus[bought] = newTimer;
                return bought;
            }
            _timeBus.Add(newTimer);
            return _timeBus.Count - 1;
        }

        //Gets the current time for the given timer
        public static double GetCurrentTime(int id, bool flush=false)
        {
            var tmp = _timeBus[id].TimerCurrent;
            if (flush) _timeBus[id].FlushTime();
            return tmp;
        }

        //Gets the percentage done for the given timer. This is only applicable for one cycle and non-applicable for stopwatches 
        public static double GetPercentageDone(int id)
        {
            var time = _timeBus[id].TimerCurrent / _timeBus[id].TimerMaxSec;
            if (time > 1) return 1;
            if (time < 0) return 0;
            return time;
        }

        //Gets the amount of times the given timer has achieved its time goal. Resets the counter back to zero by default
        public static int GetNumberOfFires(int id, bool flush=true)
        {
            var count = _timeBus[id].FireCount;
            if (flush) _timeBus[id].FlushTimer();
            return count;
        }

        //Checks if there's been at least one fire but only decreases the count by one rather than a full reset
        public static bool GetAFire(int id)
        {
            if (_timeBus[id].FireCount <= 0) return false;
            _timeBus[id].FireCount--;
            return true;
        }

        //Checks if the given timer is on
        public static bool GetStatus(int id)
        {
            return _timeBus[id].On;
        }

        //Gets the last frame time of the given timer
        public static double GetLastTickTime(int id)
        {
            return _timeBus[id].On ? _timeBus[id].LastTickTime : 0;
        }

        //Turns off the given timers
        public static void TurnOffTimer(params int[] ids)
        {
            foreach (var id in ids)
            {
                _timeBus[id].On = false;
            }
        }
        //Turns on the given timers
        public static void TurnOnTimer(params int[] ids)
        {
            foreach (var id in ids)
            {
                _timeBus[id].On = true;
            }
        }

        //Sets the time goal for a given timer (rather than having to Get a new one)
        public static void RecycleTimer(int id, double newMaxTime)
        {
            _timeBus[id] = new Timer(newMaxTime, _timeBus[id].System);
        }

        //Resets a given timer (time and fire count). This does not affect if the timer is on or off
        public static void ResetTimer(int id)
        {
            _timeBus[id].FlushTimer();
            _timeBus[id].TimerCurrent = 0;
        }

        //Update all timers on the bus
        public static void Update(GameTime gameTime)
        {
            //Sometimes timers get added in async, so it's easier just to do this rather than add a lock
            for (var i = 0; i < _timeBus.Count; i++)
            {
                _timeBus[i].Update(gameTime);
            }
        }

        //Mark given timers as no longer in use so they can be redistributed 
        public static void SellTimer(params int[] ids)
        {
            TurnOffTimer(ids);
            _timersForSale.AddRange(ids);
        }

        //Toggle all the non-system timers on or off. Useful for pausing things
        public static void ToggleAllNonSystemTimers(bool on)
        {
            //Warning: This may have unexpected behavior
            foreach (var t in _timeBus)
                if (!t.System) t.On = on;
        }
    }

    public class Timer
    {
        public readonly double TimerMaxSec; //Time being counted towards. None if less than zero
        public double TimerCurrent;         //Current time on the timer
        public int FireCount;               //Number of times the timer has reached TimerMaxSec since last reset. N/A if TimerMaxSec < 0
        public bool On, System;             //On: Timer is on. System: Timer is a system timer
        public double LastTickTime;         //Amount of time passed from the most recent update to the prior frame. Does not take into account time spent off

        public Timer(double maxTimeSeconds, bool systemTimer)
        {
            TimerMaxSec = maxTimeSeconds;
            FireCount = 0;
            TimerCurrent = 0;
            On = false; //Timers are off by default
            System = systemTimer;
        }

        public void FlushTime()
        {
            TimerCurrent %= TimerMaxSec;
        }

        public void FlushTimer()
        {
            FireCount = 0;
            FlushTime();
        }

        public void Update(GameTime gameTime)
        {
            if (!On) return;
            LastTickTime = gameTime.ElapsedGameTime.TotalSeconds;
            TimerCurrent += LastTickTime;
            if (TimerMaxSec > 0 && TimerCurrent >= TimerMaxSec)
            {
                FireCount += (int)(TimerCurrent / TimerMaxSec);
            }
        }
    }

}
