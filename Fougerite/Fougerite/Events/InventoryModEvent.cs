
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fougerite.Events
{
    public class InventoryModEvent
    {
        private Inventory _inventory;
        private int _slot;
        private IInventoryItem _item;
        private Fougerite.Player _player = null;
        private NetUser _netuser = null;
        private uLink.NetworkPlayer _netplayer;

        public InventoryModEvent(Inventory inventory, int slot, IInventoryItem item)
        {
            this._inventory = inventory;
            this._slot = slot;
            this._item = item;
            foreach (uLink.NetworkPlayer netplayer in inventory._netListeners)
            {
                NetUser user = netplayer.GetLocalData() as NetUser;
                if (user != null)
                {
                    _netuser = user;
                    _player = Fougerite.Server.Cache[_netuser.userID];
                    _netplayer = netplayer;
                    break;
                }
            }
        }

        public Fougerite.Player Player
        {
            get { return _player; }
        }

        public NetUser NetUser
        {
            get { return _netuser; }
        }

        public uLink.NetworkPlayer NetPlayer
        {
            get { return _netplayer; }
        }

        public IInventoryItem InventoryItem
        {
            get { return _item; }
        }

        public EntityItem Item
        {
            get { return new EntityItem(_inventory, _slot); }
        }

        public int Slot
        {
            get { return _slot; }
        }

        public Inventory Inventory
        {
            get { return _inventory; }
        }

        public FInventory FInventory
        {
            get { return new FInventory(_inventory); }
        }
    }
}
