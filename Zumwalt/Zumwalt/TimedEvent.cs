namespace Zumwalt
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Timers;

    public class TimedEvent
    {
        private string _name;
        private System.Timers.Timer _timer;

        public event TimedEventFireDelegate OnFire;

        public TimedEvent(string name, double interval)
        {
            this._name = name;
            this._timer = new System.Timers.Timer();
            this._timer.Interval = interval;
            this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.OnFire != null)
            {
                this.OnFire(this.Name);
            }
        }

        public void Start()
        {
            this._timer.Start();
        }

        public void Stop()
        {
            this._timer.Stop();
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

        public delegate void TimedEventFireDelegate(string name);
    }
}

