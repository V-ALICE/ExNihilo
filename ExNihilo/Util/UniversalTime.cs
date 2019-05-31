using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    public static class UniversalTime
    {
        //Timers that are being tracked are added to the bus. All active timers on the bus are updated per time step
        private static readonly List<Timer> TimeBus = new List<Timer>();
        //Timers that are no longer needed by their Geter have their seats on the bus sold back for the next Geter
        private static readonly List<int> TimersForSale = new List<int>();

        public static int NewTimer(bool systemTimer, double maxTimeSec=-1)
        {
            var newTimer = new Timer(maxTimeSec, systemTimer);
            if (TimersForSale.Count > 0)
            {
                var bought = TimersForSale[0];
                TimersForSale.RemoveAt(0);
                TimeBus[bought] = newTimer;
                return bought;
            }
            TimeBus.Add(newTimer);
            return TimeBus.Count - 1;
        }

        //Gets the current time for the given timer
        public static double GetCurrentTime(int id, bool flush=false)
        {
            ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
            var tmp = TimeBus[id].TimerCurrent;
            if (flush) TimeBus[id].FlushTime();
            return tmp;
        }

        //Gets the percentage done for the given timer. This is only applicable for one cycle and non-applicable for stopwatches 
        public static double GetPercentageDone(int id)
        {
            ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
            var time = TimeBus[id].TimerCurrent / TimeBus[id].TimerMaxSec;
            if (time > 1) return 1;
            if (time < 0) return 0;
            return time;
        }

        //Gets the amount of times the given timer has achieved its time goal. Resets the counter back to zero by default
        public static int GetNumberOfFires(int id, bool flush=true)
        {
            ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
            var count = TimeBus[id].FireCount;
            if (flush) TimeBus[id].FlushTimer();
            return count;
        }

        //Checks if there's been at least one fire but only decreases the count by one rather than a full reset
        public static bool GetAFire(int id)
        {
            ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
            if (TimeBus[id].FireCount <= 0) return false;
            TimeBus[id].FireCount--;
            return true;
        }

        //Checks if the given timer is on
        public static bool GetStatus(int id)
        {
            ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
            return TimeBus[id].On;
        }

        //Gets the last frame time of the given timer
        public static double GetLastTickTime(int id)
        {
            ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
            return TimeBus[id].On ? TimeBus[id].LastTickTime : 0;
        }

        //Turns off the given timers
        public static void TurnOffTimer(params int[] ids)
        {
            foreach (var id in ids)
            {
                ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
                TimeBus[id].On = false;
            }
        }
        //Turns on the given timers
        public static void TurnOnTimer(params int[] ids)
        {
            foreach (var id in ids)
            {
                ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
                TimeBus[id].On = true;
            }
        }

        //Sets the time goal for a given timer (rather than having to Get a new one)
        public static void RecycleTimer(int id, double newMaxTime)
        {
            ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
            TimeBus[id] = new Timer(newMaxTime, TimeBus[id].System);
        }

        //Resets a given timer (time and fire count). This does not affect if the timer is on or off
        public static void ResetTimer(int id)
        {
            ExceptionCheck.AssertCondition(id < TimeBus.Count, "ID=" + id + " TimeBus.Count=" + TimeBus.Count);
            TimeBus[id].FlushTimer();
            TimeBus[id].TimerCurrent = 0;
        }

        //Update all timers on the bus
        public static void Update(GameTime gameTime)
        {
            //Sometimes timers get added in async, so it's easier just to do this rather than add a lock
            for (var i = 0; i < TimeBus.Count; i++)
            {
                TimeBus[i].Update(gameTime);
            }
        }

        //Mark given timers as no longer in use so they can be redistributed 
        public static void SellTimer(params int[] ids)
        {
            TurnOffTimer(ids);
            TimersForSale.AddRange(ids);
        }

        //Toggle all the non-system timers on or off. Useful for pausing things
        public static void ToggleAllNonSystemTimers(bool on)
        {
            //Warning: This may have unexpected behavior
            foreach (var t in TimeBus)
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
