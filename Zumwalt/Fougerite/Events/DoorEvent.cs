namespace Fougerite.Events
{
    using Fougerite;
    using System;

    public class DoorEvent
    {
        private Fougerite.Entity _ent;
        private bool _open;

        public DoorEvent(Fougerite.Entity e)
        {
            this.Open = false;
            this.Entity = e;
        }

        public Fougerite.Entity Entity
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