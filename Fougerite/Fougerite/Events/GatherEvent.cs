using System.Diagnostics.Contracts;

namespace Fougerite.Events
{
    using System;

    public class GatherEvent
    {
        private string _item;
        private bool _over;
        private int _qty;
        private readonly string _type;
        private readonly ResourceTarget res;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(res != null);
            Contract.Invariant(_type != null);
            Contract.Invariant(_item != null);
            Contract.Invariant(_qty >= 0);
        }

        public GatherEvent(ResourceTarget r, ItemDataBlock db, int qty)
        {
            Contract.Requires(r != null);
            Contract.Requires(db != null);
            Contract.Requires(qty >= 0);

            if (db.name == null)
                throw new InvalidOperationException("ItemDataBlock's name is null.");

            this.res = r;
            this._qty = qty;
            this._item = db.name;
            this._type = "Tree";
            this.Override = false;
        }

        public GatherEvent(ResourceTarget r, ResourceGivePair gp, int qty)
        {
            Contract.Requires(r != null);
            Contract.Requires(gp != null);
            Contract.Requires(qty >= 0);

            if (gp.ResourceItemDataBlock == null)
                throw new InvalidOperationException("ResourceGivePair's ResourceItemDataBlock property is null.");
            if (gp.ResourceItemDataBlock.name == null)
                throw new InvalidOperationException("ResourceItemDataBlock's name is null.");

            this.res = r;
            this._qty = qty;
            this._item = gp.ResourceItemDataBlock.name;
            this._type = this.res.type.ToString();
            this.Override = false;
        }

        public int AmountLeft
        {
            get
            {
                return this.res.GetTotalResLeft();
            }
        }

        public string Item
        {
            get
            {
                return this._item;
            }
            set
            {
                Contract.Requires(value != null);
                this._item = value;
            }
        }

        public bool Override
        {
            get
            {
                return this._over;
            }
            set
            {
                this._over = value;
            }
        }

        public float PercentFull
        {
            get
            {
                return this.res.GetPercentFull();
            }
        }

        public int Quantity
        {
            get
            {
                return this._qty;
            }
            set
            {
                Contract.Requires(value >= 0);
                this._qty = value;
            }
        }

        public string Type
        {
            get
            {
                return this._type;
            }
        }
    }
}