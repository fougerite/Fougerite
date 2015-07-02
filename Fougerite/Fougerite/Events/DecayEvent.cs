namespace Fougerite.Events
{

    public class DecayEvent
    {
        private float _dmg;
        private Fougerite.Entity _ent;

        public DecayEvent(Fougerite.Entity en, ref float dmg)
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
    }
}