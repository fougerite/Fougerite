namespace Zumwalt
{
    using System;

    public class HurtEvent
    {
        private Zumwalt.Player _attacker;
        private DamageEvent _de;
        private Zumwalt.Entity _ent;
        private Zumwalt.Player _victim;
        private string _weapon;
        private WeaponImpact _wi;

        public HurtEvent(ref DamageEvent d)
        {
            Zumwalt.Player player = Zumwalt.Player.FindByPlayerClient(d.attacker.client);
            if (player != null)
            {
                this.Attacker = player;
            }
            else
            {
                this.Attacker = new NPC(d.attacker.character);
            }
            this.Victim = Zumwalt.Player.FindByPlayerClient(d.victim.client);
            this.DamageEvent = d;
            this.WeaponData = null;
            if (d.extraData != null)
            {
                WeaponImpact extraData = d.extraData as WeaponImpact;
                this.WeaponData = extraData;
                string name = "";
                if (extraData.dataBlock != null)
                {
                    name = extraData.dataBlock.name;
                }
                this.WeaponName = name;
            }
        }

        public HurtEvent(ref DamageEvent d, Zumwalt.Entity en) : this(ref d)
        {
            this.Entity = en;
        }

        public Zumwalt.Player Attacker
        {
            get
            {
                return this._attacker;
            }
            set
            {
                this._attacker = value;
            }
        }

        public float DamageAmount
        {
            get
            {
                return this._de.amount;
            }
            set
            {
                this._de.amount = value;
            }
        }

        public DamageEvent DamageEvent
        {
            get
            {
                return this._de;
            }
            set
            {
                this._de = value;
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

        public Zumwalt.Player Victim
        {
            get
            {
                return this._victim;
            }
            set
            {
                this._victim = value;
            }
        }

        public WeaponImpact WeaponData
        {
            get
            {
                return this._wi;
            }
            set
            {
                this._wi = value;
            }
        }

        public string WeaponName
        {
            get
            {
                return this._weapon;
            }
            set
            {
                this._weapon = value;
            }
        }
    }
}

