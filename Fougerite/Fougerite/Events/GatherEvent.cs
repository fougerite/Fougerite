namespace Fougerite.Events
{
    public class GatherEvent
    {
        private string _item;
        private bool _over;
        private int _qty;
        private string _type;
        private ResourceTarget res;
        private ItemDataBlock dataBlock = null;
        private ResourceGivePair resourceGivePair = null;

        public GatherEvent(ResourceTarget r, ItemDataBlock db, int qty)
        {
            this.res = r;
            this._qty = qty;
            this._item = db.name;
            this._type = "Tree";
            this.dataBlock = db;
            this.Override = false;
        }

        public GatherEvent(ResourceTarget r, ResourceGivePair gp, int qty)
        {
            this.res = r;
            this._qty = qty;
            this._item = gp.ResourceItemDataBlock.name;
            this._type = this.res.type.ToString();
            this.resourceGivePair = gp;
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

        public ResourceTarget ResourceTarget
        {
            get
            {
                return this.res;
            }
        }

        public ItemDataBlock ItemDataBlock
        {
            get
            {
                return this.dataBlock;
            }
        }

        public ResourceGivePair ResourceGivePair
        {
            get
            {
                return this.resourceGivePair;
            }
        }
    }
}