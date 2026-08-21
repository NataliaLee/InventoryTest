using Assets.Scripts.InventoryLogic.Items;
using System;

namespace Assets.Scripts.InventoryLogic.Inventory
{

    public sealed class InventoryEntry
    {
        public Item Item { get; }

        public int Amount { get; internal set; }

        internal InventoryEntry(Item item, int amount)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            Amount = amount;
        }

        internal InventoryEntry Clone()
        {
            return new InventoryEntry(Item, Amount);
        }
    }
}
