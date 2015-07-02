namespace Fougerite
{

    public class EntityInv
    {
        private readonly Fougerite.Entity entity;
        private readonly EntityItem[] _items;
        private readonly Inventory _inv;

        public EntityInv(Inventory inv, Entity ent)
        {
            this.entity = ent;
            this._inv = inv;

            this._items = new EntityItem[inv.slotCount];
            for (var i = 0; i < inv.slotCount; i++)
                this._items[i] = new EntityItem(this._inv, i);
        }

        public void AddItem(string name)
        {
            this.AddItem(name, 1);
        }

        public void AddItem(string name, int amount)
        {
            ItemDataBlock item = DatablockDictionary.GetByName(name);
            this._inv.AddItemAmount (item, amount);
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

        public void ClearAll()
        {
            this._inv.Clear();
        }

        public Fougerite.Entity Entity
        {
            get
            {
                return this.entity;
            }
        }

        private int GetFreeSlots ()
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

        public bool HasItem(string name, int amount = 1)
        {
            int num = 0;
            foreach (EntityItem item in this.Items)
            {
                if (item.Name == name)
                    num += item.UsesLeft;
            }
            return (num >= amount);
        }

        public void MoveItem(int s1, int s2)
        {
            this._inv.MoveItemAtSlotToEmptySlot(this._inv, s1, s2);
        }

        public void RemoveItem (string name, int amount = 1)
        {
            foreach (EntityItem item in this.Items)
            {
                if (item.Name == name)
                {
                    if (item.UsesLeft > amount)
                    {
                        this._inv.RemoveItem(item.RInventoryItem);
                        this.AddItem(item.Name, (item.UsesLeft - amount));
                        return;
                    }
                    else if (item.UsesLeft == amount)
                    {
                        this._inv.RemoveItem(item.RInventoryItem);
                        return;
                    }
                    else
                    {
                        this._inv.RemoveItem(item.RInventoryItem);
                        amount -= item.UsesLeft;
                    }
                }    
            }
        }

        public void RemoveItem (int slot, int amount = 1)
        {
            EntityItem item = this.Items [slot];
            if (item == null)
                return;
            if (item.UsesLeft > amount)
            {
                this._inv.RemoveItem (item.RInventoryItem);
                this.AddItem (item.Name, (item.UsesLeft - amount));
                return;
            }
            this._inv.RemoveItem (item.RInventoryItem);
        }

        public int FreeSlots
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
        }

        public Inventory InternalInventory
        {
            get
            {
                return this._inv;
            }
        }

        public EntityItem[] Items
        {
            get
            {
                return this._items;
            }
        }
    }
}
