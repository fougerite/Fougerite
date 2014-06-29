namespace Zumwalt.Events
{
    using Zumwalt;
    using System;

    public class DecayEvent
    {
        private float _dmg;
        private Zumwalt.Entity _ent;

        public DecayEvent(Zumwalt.Entity en, ref float dmg)
        {
            this.Entity = en;
            this.DamageAmount = dmg;
        }

        public float DamageAmount
        {
            get
            {
                return this._dmg;
            }
            set
            {
                this._dmg = value;
            }
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
    }
}