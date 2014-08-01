using System.Diagnostics.Contracts;

namespace Fougerite.Events
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Timers;

    public class TimedEvent
    {
        private object[] _args;
        private readonly string _name;
        private readonly System.Timers.Timer _timer;
        private long lastTick;

        public event TimedEventFireDelegate OnFire;

        public event TimedEventFireArgsDelegate OnFireArgs;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_timer != null);
            Contract.Invariant(!string.IsNullOrEmpty(_name));
        }

        public TimedEvent(string name, double interval)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(interval >= 0);

            this._name = name;
            this._timer = new System.Timers.Timer();
            this._timer.Interval = interval;
            this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
        }

        public TimedEvent(string name, double interval, object[] args)
            : this(name, interval)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(interval >= 0);
            Contract.Requires(args != null);
            this.Args = args;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
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

        public void Start()
        {
            this._timer.Start();
            this.lastTick = DateTime.UtcNow.Ticks;
        }

        public void Stop()
        {
            this._timer.Stop();
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