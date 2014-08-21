namespace Fougerite
{
    using System;

    public class EntityInv
    {
        private Inventory _inv;
        private EntityItem[] _items;
        private Fougerite.Entity entity;

        public EntityInv(Fougerite.Entity entity)
        {
            this.entity = entity;
            this._inv = entity.DirectInventory;
            //this.InitItems();
        }

        public void AddItem(string name)
        {
            this.AddItem(name, 1);
        }

        public void AddItem(string name, int amount)
        {
            ItemDataBlock byName = DatablockDictionary.GetByName(name);
            if (byName != null) this._inv.AddItemAmount(byName, amount);
        }

        public void AddItemTo(string name, int slot)
        {
            this.AddItemTo(name, slot, 1);
        }

        public void AddItemTo(string name, int slot, int amount)
        {
            ItemDataBlock byName = DatablockDictionary.GetByName(name);
            if (byName != null)
            {
                Inventory.Slot.Kind place = Inventory.Slot.Kind.Default;
                this._inv.AddItemSomehow(byName, new Inventory.Slot.Kind?(place), slot, amount);
            }
        }

        public void Clear()
        {
            foreach (EntityItem item in this.Items)
            {
                this._inv.RemoveItem(item.RInventoryItem);
            }
        }

        public void ClearAll()
        {
            this._inv.Clear();
        }

        private int GetFreeSlots()
        {
            int num = 0;
            for (int i = 0; i < this._inv.slotCount; i++)
            {
                if (this._inv.IsSlotFree(i))
                {
                    num++;
                }
            }
            return num;
        }

        public bool HasItem(string name)
        {
            return this.HasItem(name, 1);
        }

        public bool HasItem(string name, int number)
        {
            int num = 0;
            foreach (EntityItem item in this.Items)
            {
                if (item.Name == name)
                {
                    if (item.UsesLeft >= number)
                    {
                        return true;
                    }
                    num += item.UsesLeft;
                }
            }
            return (num >= number);
        }

        //Todo: Needs to be worked out  (Make SlotCount not to be undefined)
        /*private void InitItems()
        {
            int slotc = this._inv.slotCount;
            this.Items = new EntityItem[slotc];
            for (int i = 0; i < slotc; i++)
            {
                if (i < slotc) this.Items[i] = new EntityItem(ref this._inv, i);
            }
        }*/

        public void MoveItem(int s1, int s2)
        {
            this._inv.MoveItemAtSlotToEmptySlot(this._inv, s1, s2);
        }

        public void RemoveItem(EntityItem pi)
        {
            foreach (EntityItem item in this.Items)
            {
                if (item == pi)
                {
                    this._inv.RemoveItem(pi.RInventoryItem);
                    return;
                }
            }
        }

        public void RemoveItem(int slot)
        {
            this._inv.RemoveItem(slot);
        }

        public void RemoveItem(string name, int number)
        {
            int qty = number;
            foreach (EntityItem item in this.Items)
            {
                if (item.Name == name)
                {
                    if (item.UsesLeft > qty)
                    {
                        item.Consume(qty);
                        qty = 0;
                        break;
                    }
                    qty -= item.UsesLeft;
                    if (qty < 0)
                    {
                        qty = 0;
                    }
                    this._inv.RemoveItem(item.Slot);
                    if (qty == 0)
                    {
                        break;
                    }
                }
            }
        }

        public void RemoveItemAll(string name)
        {
            this.RemoveItem(name, 0x1869f);
        }

        //Todo: Needs to be worked out (Make SlotCount not to be undefined)
        /*public int FreeSlots
        {
            get
            {
                return this.GetFreeSlots();
            }
        }

        public int SlotCount
        {
            get
            {
                return this._inv.slotCount;
            }
        }*/

        public EntityItem[] Items
        {
            get
            {
                return this._items;
            }
            set
            {
                this._items = value;
            }
        }
    }
}