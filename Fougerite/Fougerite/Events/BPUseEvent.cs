using System.Diagnostics.Contracts;

namespace Fougerite.Events
{
    using System;

    public class BPUseEvent
    {
        private BlueprintDataBlock _bdb;
        private bool _cancel;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_bdb != null);
        }

        public BPUseEvent(BlueprintDataBlock bdb)
        {
            Contract.Requires(bdb != null);

            this.DataBlock = bdb;
            this.Cancel = false;
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
                Contract.Requires(value != null);
                this._bdb = value;
            }
        }

        public string ItemName
        {
            get
            {
                if (_bdb.resultItem == null)
                    throw new InvalidOperationException("BlueprintDataBlock's resultItem field is null.");
                return this._bdb.resultItem.name;
            }
        }
    }
}