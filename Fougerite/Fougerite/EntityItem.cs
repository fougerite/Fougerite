namespace Fougerite
{
    using System;

    public class EntityItem
    {
        private Inventory internalInv;
        private int internalSlot;

        public EntityItem()
        {
        }

        public EntityItem(ref Inventory inv, int slot)
        {
            this.internalInv = inv;
            this.internalSlot = slot;
        }

        private IInventoryItem GetItemRef()
        {
            IInventoryItem item;
            this.internalInv.GetItem(this.internalSlot, out item);
            return item;
        }

        public void Consume(int qty)
        {
            if (!this.IsEmpty())
            {
                this.RInventoryItem.Consume(ref qty);
            }
        }

        public bool IsEmpty()
        {
            return (this.RInventoryItem == null);
        }

        public bool TryCombine(EntityItem pi)
        {
            if (this.IsEmpty() || pi.IsEmpty())
            {
                return false;
            }
            return (this.RInventoryItem.TryCombine(pi.RInventoryItem) != InventoryItem.MergeResult.Failed);
        }

        public bool TryStack(EntityItem pi)
        {
            if (this.IsEmpty() || pi.IsEmpty())
            {
                return false;
            }
            return (this.RInventoryItem.TryStack(pi.RInventoryItem) != InventoryItem.MergeResult.Failed);
        }

        public IInventoryItem RInventoryItem
        {
            get
            {
                return this.GetItemRef();
            }
            set
            {
                this.RInventoryItem = value;
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
                return null;
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
                return -1;
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