﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace JintPlugin
{
    public class JintTE
    {
        private Dictionary<string, object> _args;
		private readonly string _name;
		private readonly System.Timers.Timer _timer;
		private long lastTick;
		private int _elapsedCount;

		public delegate void TimedEventFireDelegate(JintTE evt);

		public event TimedEventFireDelegate OnFire;

		public JintTE(string name, double interval) {
			this._name = name;
			this._timer = new System.Timers.Timer();
			this._timer.Interval = interval;
			this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
			this._elapsedCount = 0;
		}

        public JintTE(string name, double interval, Dictionary<string, object> args)
			: this(name, interval) {
			this.Args = args;
		}

		private void _timer_Elapsed(object sender, ElapsedEventArgs e) {
			if (this.OnFire != null) {
				this.OnFire(this);
			}

			this._elapsedCount += 1;
			this.lastTick = DateTime.UtcNow.Ticks;
		}

		public void Start() {
			this._timer.Start();
			this.lastTick = DateTime.UtcNow.Ticks;
		}

		public void Stop() {
			this._timer.Stop();
		}

		public void Kill() {
			this._timer.Stop();
			this._timer.Dispose();
		}

		public Dictionary<string, object> Args {
			get { return this._args; }
			set { this._args = value; }
		}

		public double Interval {
			get { return this._timer.Interval; }
			set { this._timer.Interval = value; }
		}

		public string Name {
			get { return this._name; }
		}

		public double TimeLeft {
			get { return (this.Interval - ((DateTime.UtcNow.Ticks - this.lastTick) / 0x2710L)); }
		}

		public int ElapsedCount {
			get { return this._elapsedCount; }
		}
    }
}
