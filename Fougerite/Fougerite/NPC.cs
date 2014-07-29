using System.Diagnostics.Contracts;

namespace Fougerite
{
    using System;
    using UnityEngine;

    public class NPC
    {
        private readonly Character _char;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_char != null);
        }

        public NPC(Character c)
        {
            Contract.Requires(c != null);

            this._char = c;
        }

        public void Kill()
        {
            this.Character.Signal_ServerCharacterDeath();
            this.Character.SendMessage("OnKilled", new DamageEvent(), SendMessageOptions.DontRequireReceiver);
        }

        public Character Character
        {
            get
            {
                return this._char;
            }
        }

        public float Health
        {
            get
            {
                return this._char.health;
            }
            set
            {
                if (_char.takeDamage == null)
                    throw new InvalidOperationException("NPC's takeDamage field is null.");
                this._char.takeDamage.health = value;
            }
        }

        public string Name
        {
            get
            {
                if (_char.name == null)
                    throw new InvalidOperationException("NPC's name is null.");
                return this._char.name.Replace("(Clone)", "");
            }
            set
            {
                this._char.name = value;
            }
        }
    }
}