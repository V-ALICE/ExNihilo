using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ExNihilo.Util
{
    public static class UniversalTime
    {
        //Timers that are being tracked are added to the bus. All active timers on the bus are updated per time step
        private static readonly List<Timer> TimeBus = new List<Timer>();
        //Timers that are no longer needed by their requester have their seats on the bus sold back for the next requester
        private static readonly List<int> TimersForSale = new List<int>();

        public static int RequestTimer(bool systemTimer, double maxTimeSec=-1)
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
        public static double RequestCurrentTime(int ID)
        {
            ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
            return TimeBus[ID].TimerCurrent;
        }

        //Gets the percentage done for the given timer. This is only applicable for one cycle and non-applicable for stopwatches 
        public static double RequestPercentageDone(int ID)
        {
            ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
            var time = TimeBus[ID].TimerCurrent / TimeBus[ID].TimerMaxSec;
            if (time > 1) return 1;
            if (time < 0) return 0;
            return time;
        }

        //Gets the amount of times the given timer has achieved its time goal. Resets the counter back to zero by default
        public static int RequestNumberOfFires(int ID, bool reset=true)
        {
            ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
            var count = TimeBus[ID].FireCount;
            if (reset) TimeBus[ID].FireCount = 0;
            return count;
        }

        //Checks if there's been at least one fire but only decreases the count by one rather than a full reset
        public static bool RequestAFire(int ID)
        {
            ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
            if (TimeBus[ID].FireCount <= 0) return false;
            TimeBus[ID].FireCount--;
            return true;
        }

        //Checks if the given timer is on
        public static bool RequestStatus(int ID)
        {
            ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
            return TimeBus[ID].On;
        }

        //Gets the last frame time of the given timer
        public static double RequestLastTickTime(int ID)
        {
            ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
            return TimeBus[ID].On ? TimeBus[ID].LastTickTime : 0;
        }

        //Turns off the given timers
        public static void TurnOffTimer(params int[] IDs)
        {
            foreach (var ID in IDs)
            {
                ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
                TimeBus[ID].On = false;
            }
        }
        //Turns on the given timers
        public static void TurnOnTimer(params int[] IDs)
        {
            foreach (var ID in IDs)
            {
                ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
                TimeBus[ID].On = true;
            }
        }

        //Sets the time goal for a given timer (rather than having to request a new one)
        public static void RecycleTimer(int ID, double newMaxTime)
        {
            ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
            TimeBus[ID] = new Timer(newMaxTime, TimeBus[ID].System);
        }

        //Resets a given timer (time and fire count). This does not affect if the timer is on or off
        public static void ResetTimer(int ID)
        {
            ExceptionCheck.AssertCondition(ID < TimeBus.Count, "ID=" + ID + " TimeBus.Count=" + TimeBus.Count);
            TimeBus[ID].ClearTime();
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
        public static void SellTimer(params int[] ID)
        {
            TurnOffTimer(ID);
            TimersForSale.AddRange(ID);
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

        public void ClearTime()
        {
            TimerCurrent = 0;
            FireCount = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (!On) return;
            LastTickTime = gameTime.ElapsedGameTime.TotalSeconds;
            TimerCurrent += LastTickTime;
            if (TimerMaxSec > 0 && TimerCurrent >= TimerMaxSec)
            {
                FireCount += (int)(TimerCurrent / TimerMaxSec);
                TimerCurrent %= TimerMaxSec;
            }
        }
    }

}
