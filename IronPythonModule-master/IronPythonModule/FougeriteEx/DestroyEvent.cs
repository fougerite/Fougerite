namespace IronPythonModule.Events
{
	using System;
	using Fougerite;

	public class DestroyEvent
	{
		private object _destroyer;
		private DamageEvent _de;
		private bool _decay;
		private Fougerite.Entity _ent;
		private string _weapon;
		private WeaponImpact _wi;

		public DestroyEvent(ref DamageEvent d, Entity ent, bool isdecay)
		{
			Fougerite.Player player = Fougerite.Player.FindByPlayerClient(d.attacker.client);
			if (player != null) {
				this.Destroyer = player;
			} else {
				this.Destroyer = new NPC(d.attacker.character);
			}

			this.WeaponData = null;
			this.IsDecay = isdecay;
			this.DamageEvent = d;
			this.Entity = ent;

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

		public object Destroyer
		{
			get
			{
				return this._destroyer;
			}
			set
			{
				this._destroyer = value;
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

