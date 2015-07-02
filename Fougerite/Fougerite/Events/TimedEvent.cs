namespace Fougerite.Events
{
    using System;
    using System.Timers;

    public class TimedEvent
    {
        private object[] _args;
        private string _name;
        private System.Timers.Timer _timer;
        private long lastTick;

        public event TimedEventFireDelegate OnFire;

        public event TimedEventFireArgsDelegate OnFireArgs;

        public TimedEvent(string name, double interval)
        {
            this._name = name;
            this._timer = new System.Timers.Timer();
            this._timer.Interval = interval;
            this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
        }

        public TimedEvent(string name, double interval, object[] args)
            : this(name, interval)
        {
            this.Args = args;
        }

        public TimedEvent(string name, double interval, bool flag)
            : this(name, interval)
        {
            this._timer.AutoReset = flag;
        }

        public TimedEvent(string name, double interval, object[] args, bool flag)
            : this(name, interval, args)
        {
            this._timer.AutoReset = flag;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (this.OnFire != null)
                {
                    this.OnFire(this.Name);
                }
                if (this.OnFireArgs != null)
                {
                    this.OnFireArgs(this.Name, this.Args);
                }
                this.lastTick = DateTime.UtcNow.Ticks;
            }
            catch (Exception ex)
            {
                Logger.LogDebug("Error occured at timer: " + this.Name + " Error: " + ex.ToString());
                this.Stop();
                Logger.LogDebug("Trying to restart timer.");
                this.Start();
                Logger.LogDebug("Restarted!");
            }
        }

        public void Start()
        {
            this._timer.Start();
            this.lastTick = DateTime.UtcNow.Ticks;
        }

        public void Stop()
        {
            this._timer.Stop();
        }

        public void Kill()
        {
            Stop();
            this._timer.Dispose();
        }

        public bool AutoReset
        {
            get { return this._timer.AutoReset; }
            set { this._timer.AutoReset = value; }
        }

        public object[] Args
        {
            get
            {
                return this._args;
            }
            set
            {
                this._args = value;
            }
        }

        
        public double Interval
        {
            get
            {
                return this._timer.Interval;
            }
            set
            {
                this._timer.Interval = value;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public double TimeLeft
        {
            get
            {
                return (this.Interval - ((DateTime.UtcNow.Ticks - this.lastTick) / 0x2710L));
            }
        }

        public delegate void TimedEventFireArgsDelegate(string name, object[] list);

        public delegate void TimedEventFireDelegate(string name);
    }
}