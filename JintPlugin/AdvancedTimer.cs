using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JintPlugin
{
    public class AdvancedTimer
    {
        public readonly Dictionary<string, JintTE> Timers;
        public readonly List<JintTE> ParallelTimers;
        public Plugin Plugin;

        public AdvancedTimer(Plugin pl)
        {
            Plugin = pl;
            Timers = new Dictionary<string, JintTE>();
            ParallelTimers = new List<JintTE>();
        }

        #region timer methods

        public JintTE CreateTimer(string name, int timeoutDelay)
        {
            JintTE timedEvent = GetTimer(name);
            if (timedEvent == null)
            {
                timedEvent = new JintTE(name, (double)timeoutDelay);
                timedEvent.OnFire += new JintTE.TimedEventFireDelegate(Plugin.OnTimerCB2);
                Timers.Add(name, timedEvent);
            }
            return timedEvent;
        }

        public JintTE CreateTimer(string name, int timeoutDelay, Dictionary<string, object> args)
        {
            JintTE timedEvent = GetTimer(name);
            if (timedEvent == null)
            {
                timedEvent = new JintTE(name, (double)timeoutDelay);
                timedEvent.Args = args;
                timedEvent.OnFire += new JintTE.TimedEventFireDelegate(Plugin.OnTimerCB2);
                Timers.Add(name, timedEvent);
            }
            return timedEvent;
        }

        public JintTE GetTimer(string name)
        {
            JintTE result;
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
            JintTE timer = GetTimer(name);
            if (timer == null)
                return;

            timer.Kill();
            Timers.Remove(name);
        }

        public void KillTimers()
        {
            foreach (JintTE current in Timers.Values)
            {
                current.Kill();
            }
            foreach (JintTE timer in ParallelTimers)
            {
                timer.Kill();
            }
            Timers.Clear();
            ParallelTimers.Clear();
        }

        #endregion

        #region ParalellTimers

        public JintTE CreateParallelTimer(string name, int timeoutDelay, Dictionary<string, object> args)
        {
            JintTE timedEvent = new JintTE(name, (double)timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += new JintTE.TimedEventFireDelegate(Plugin.OnTimerCB2);
            ParallelTimers.Add(timedEvent);
            return timedEvent;
        }

        public List<JintTE> GetParallelTimer(string name)
        {
            return (from timer in ParallelTimers
                    where timer.Name == name
                    select timer).ToList();
        }

        public void KillParallelTimer(string name)
        {
            foreach (JintTE timer in GetParallelTimer(name))
            {
                timer.Kill();
                ParallelTimers.Remove(timer);
            }
        }

        #endregion
    }
}
