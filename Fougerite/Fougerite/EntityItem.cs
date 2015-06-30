namespace Fougerite
{
    public class EntityItem
    {
        private readonly Inventory internalInv;
		private readonly int internalSlot;

        public EntityItem(Inventory inv, int slot)
		{
			this.internalInv = inv;
			this.internalSlot = slot;
		}

		public void Drop()
		{
			DropHelper.DropItem(this.internalInv, this.Slot);
		}

		private IInventoryItem GetItemRef()
		{
			IInventoryItem item;
			this.internalInv.GetItem(this.internalSlot, out item);
			return item;
		}

		public bool IsEmpty()
		{
			return (this.RInventoryItem == null);
		}

		public IInventoryItem RInventoryItem
		{
			get
			{
				return this.GetItemRef();
			}
		}

		public string Name
		{
			get
			{
				if (!this.IsEmpty())
				{
					return this.RInventoryItem.datablock.name;
				}
				return "Empty slot";
			}
			set
			{
				this.RInventoryItem.datablock.name = value;
			}
		}

		public int Quantity
		{
			get
			{
				return this.UsesLeft;
			}
			set
			{
				this.UsesLeft = value;
			}
		}

		public int Slot
		{
			get
			{
				if (!this.IsEmpty())
				{
					return this.RInventoryItem.slot;
				}
				return this.internalSlot;
			}
		}

		public int UsesLeft
		{
			get
			{
				if (!this.IsEmpty())
				{
					return this.RInventoryItem.uses;
				}
				return -1;
			}
			set
			{
				this.RInventoryItem.SetUses(value);
			}
		}
    }
}
