namespace Fougerite
{
    using System;
    using System.Collections.Generic;

    public class ItemsBlocks : System.Collections.Generic.List<ItemDataBlock>
    {
        public ItemsBlocks(System.Collections.Generic.List<ItemDataBlock> items)
        {
            foreach (ItemDataBlock block in items)
            {
                base.Add(block);
            }
        }

        public ItemDataBlock Find(string str)
        {
            foreach (ItemDataBlock block in this)
            {
                if (block.name.ToLower() == str.ToLower())
                {
                    return block;
                }
            }
            return null;
        }
    }
}