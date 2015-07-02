using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagmaPlugin
{
    public class AdvancedTimer
    {
        public readonly Dictionary<string, MagmaTE> Timers;
        public readonly List<MagmaTE> ParallelTimers;
        public Plugin Plugin;

        public AdvancedTimer(Plugin pl)
        {
            Plugin = pl;
            Timers = new Dictionary<string, MagmaTE>();
            ParallelTimers = new List<MagmaTE>();
        }

        #region timer methods

        public MagmaTE CreateTimer(string name, int timeoutDelay)
        {
            MagmaTE timedEvent = GetTimer(name);
            if (timedEvent == null)
            {
                timedEvent = new MagmaTE(name, (double)timeoutDelay);
                timedEvent.OnFire += new MagmaTE.TimedEventFireDelegate(Plugin.OnTimerCB2);
                Timers.Add(name, timedEvent);
            }
            return timedEvent;
        }

        public MagmaTE CreateTimer(string name, int timeoutDelay, Dictionary<string, object> args)
        {
            MagmaTE timedEvent = GetTimer(name);
            if (timedEvent == null)
            {
                timedEvent = new MagmaTE(name, (double)timeoutDelay);
                timedEvent.Args = args;
                timedEvent.OnFire += new MagmaTE.TimedEventFireDelegate(Plugin.OnTimerCB2);
                Timers.Add(name, timedEvent);
            }
            return timedEvent;
        }

        public MagmaTE GetTimer(string name)
        {
            MagmaTE result;
            if (Timers.ContainsKey(name))
            {
                result = Timers[name];
            }
            else
            {
                result = null;
            }
            return result;
        }

        public void KillTimer(string name)
        {
            MagmaTE timer = GetTimer(name);
            if (timer == null)
                return;

            timer.Kill();
            Timers.Remove(name);
        }

        public void KillTimers()
        {
            foreach (MagmaTE current in Timers.Values)
            {
                current.Kill();
            }
            foreach (MagmaTE timer in ParallelTimers)
            {
                timer.Kill();
            }
            Timers.Clear();
            ParallelTimers.Clear();
        }

        #endregion

        #region ParalellTimers

        public MagmaTE CreateParallelTimer(string name, int timeoutDelay, Dictionary<string, object> args)
        {
            MagmaTE timedEvent = new MagmaTE(name, (double)timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += new MagmaTE.TimedEventFireDelegate(Plugin.OnTimerCB2);
            ParallelTimers.Add(timedEvent);
            return timedEvent;
        }

        public List<MagmaTE> GetParallelTimer(string name)
        {
            return (from timer in ParallelTimers
                    where timer.Name == name
                    select timer).ToList();
        }

        public void KillParallelTimer(string name)
        {
            foreach (MagmaTE timer in GetParallelTimer(name))
            {
                timer.Kill();
                ParallelTimers.Remove(timer);
            }
        }

        #endregion
    }
}
