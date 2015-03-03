namespace Fougerite.Events
{
    using Fougerite;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class HurtEvent
    {
        private object _attacker;
        private DamageEvent _de;
        private bool _decay;
        private Fougerite.Entity _ent;
        private object _victim;
        private string _weapon;
        private WeaponImpact _wi;
        private readonly bool _playerattacker;
        private readonly bool _playervictim;

        public HurtEvent(ref DamageEvent d)
        {
            Logger.LogDebug(string.Format("[DamageEvent] {0}", d.ToString()));

            this.DamageEvent = d;
            this.WeaponData = null;
            this.IsDecay = false;
            string weaponName = "Unknown";

            if (d.attacker.idMain is DeployableObject)
            {
                this.Attacker = new Entity(d.attacker.idMain.GetComponent<DeployableObject>());
                this._playerattacker = false;
                weaponName = d.attacker.id.ToString().MatchItemName();
            }
            else if (d.attacker.id is Metabolism && d.victim.id is Metabolism)
            {
                this.Attacker = Fougerite.Player.FindByPlayerClient(d.attacker.client);
                this._playerattacker = false;
                this.Victim = this.Attacker;

                ICollection<string> list = new List<string>();
                if ((this.Victim as Fougerite.Player).IsStarving)
                {
                    list.Add("Starvation");
                }
                if ((this.Victim as Fougerite.Player).IsRadPoisoned)
                {
                    list.Add("Radiation");
                }
                if ((this.Victim as Fougerite.Player).IsPoisoned)
                {
                    list.Add("Poison");
                }
                if ((this.Victim as Fougerite.Player).IsBleeding)
                {
                    list.Add("Bleeding");
                }

                if (DamageType != "Unknown" && !list.Contains(DamageType))
                    list.Add(DamageType);

                weaponName = string.Format("Self ({0})", string.Join(",", list.ToArray()));
            }
            else if (d.attacker.id is SupplyCrate)
            {
                this.Attacker = new Entity(d.attacker.idMain.gameObject);
                this._playerattacker = false;
                weaponName = "Supply Crate";
            }
            else if (d.attacker.client != null)            
            {
                this.Attacker = new Fougerite.Player(d.attacker.client);
                this._playerattacker = true;
                if (d.attacker.id is TimedExplosive)
                {
                    weaponName = "Explosive Charge";
                }
                else if (d.attacker.id is TimedGrenade)
                {
                    weaponName = "F1 Grenade";
                }
                else if (d.extraData != null)
                {
                    WeaponImpact extraData = d.extraData as WeaponImpact;
                    this.WeaponData = extraData;
                    if (extraData.dataBlock != null)
                    {
                        weaponName = extraData.dataBlock.name;
                    }
                }
                else if (d.victim.client != null && d.attacker.IsDifferentPlayer(d.victim.client))
                {
                    weaponName = "Hunting Bow";
                }
                else
                {
                    weaponName = "Fall Damage";
                }
            }
            else if (d.attacker.character != null)
            {
                this.Attacker = new NPC(d.attacker.character);
                this._playerattacker = false;
                weaponName = d.attacker.id.ToString().Replace("(Clone)", " Claw").Replace("Mutant", "Mutant ");
            }
            this.WeaponName = weaponName;

            Logger.LogDebug(string.Format("Attacker properties set: {0}, {1}, {2}", this.Attacker.ToString(), this._playerattacker.ToString(), weaponName));

            if (d.victim.idMain is DeployableObject)
            {
                Logger.LogDebug("idMain is DeployableObject");
                this.Victim = new Entity(d.victim.idMain.GetComponent<DeployableObject>());
                this._playervictim = false;
            }
            else if (d.victim.idMain is StructureComponent)
            {
                Logger.LogDebug("idMain is StructureComponent");
                this.Victim = new Entity(d.victim.idMain.GetComponent<StructureComponent>());
                this._playervictim = false;
            } 
            else if (d.victim.client != null)
            {
                this.Victim = Fougerite.Player.FindByPlayerClient(d.victim.client);
                this._playervictim = true;
            }
            else if (d.victim.character != null)
            {
                this.Victim = new NPC(d.victim.character);
                this._playervictim = false;
            }

            Logger.LogDebug(string.Format("Victim properties set: {0}, {1}", this.Victim, this._playervictim.ToString()));
        }

        public HurtEvent(ref DamageEvent d, Fougerite.Entity en)
            : this(ref d)
        {
            this.Entity = en;
        }

        public object Attacker
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

        public string DamageType
        {
            get
            {
                string str = "Unknown";
                switch (((int)this.DamageEvent.damageTypes))
                {
                    case 0:
                        return "Bleeding";

                    case 1:
                        return "Generic";

                    case 2:
                        return "Bullet";

                    case 3:
                    case 5:
                    case 6:
                    case 7:
                        return str;

                    case 4:
                        return "Melee";

                    case 8:
                        return "Explosion";

                    case 0x10:
                        return "Radiation";

                    case 0x20:
                        return "Cold";
                }
                return str;
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

        public bool IsDecay
        {
            get
            {
                return this._decay;
            }
            set
            {
                this._decay = value;
            }
        }

        public object Victim
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

        public bool VictimIsPlayer
        {
            get
            {
                return this._playervictim;
            }       
        }

        public bool AttackerIsPlayer
        {
            get
            {
                return this._playerattacker;
            }
        }
    }
}
