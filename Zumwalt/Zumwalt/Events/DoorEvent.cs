namespace Zumwalt.Events
{
    using Zumwalt;
    using System;

    public class DoorEvent
    {
        private Zumwalt.Entity _ent;
        private bool _open;

        public DoorEvent(Zumwalt.Entity e)
        {
            this.Open = false;
            this.Entity = e;
        }

        public Zumwalt.Entity Entity
        {
            get
            {
                return this._ent;
            }
            set
            {
                this._ent = value;
            }
        }

        public bool Open
        {
            get
            {
                return this._open;
            }
            set
            {
                this._open = value;
            }
        }
    }
}

