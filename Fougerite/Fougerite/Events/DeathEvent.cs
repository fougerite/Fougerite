namespace Fougerite.Events
{
    using System;

    public class DeathEvent : HurtEvent
    {
        private bool _drop;

        public DeathEvent(ref DamageEvent d)
            : base(ref d)
        {
            this.DropItems = true;
        }

        public bool DropItems
        {
            get
            {
                return this._drop;
            }
            set
            {
                Logger.LogDebug("[DropItems] DropItems is now " + value.ToString());
                this._drop = value;
            }
        }
    }
}