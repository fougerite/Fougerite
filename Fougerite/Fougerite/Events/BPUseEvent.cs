namespace Fougerite.Events
{

    public class BPUseEvent
    {
        private BlueprintDataBlock _bdb;
        private bool _cancel;
        private IBlueprintItem _item;

        public BPUseEvent(BlueprintDataBlock bdb, IBlueprintItem item)
        {
            this.DataBlock = bdb;
            this.Cancel = false;
            this._item = item;
        }

        public bool Cancel
        {
            get
            {
                return this._cancel;
            }
            set
            {
                this._cancel = value;
            }
        }

        public BlueprintDataBlock DataBlock
        {
            get
            {
                return this._bdb;
            }
            set
            {
                this._bdb = value;
            }
        }

        public IBlueprintItem Item
        {
            get
            {
                return this._item;
            }
        }

        public string ItemName
        {
            get
            {
                return this._bdb.resultItem.name;
            }
        }
    }
}