using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Items
{
    public readonly struct ContainerContent
    {
        public Item Item { get; }
        public int Amount { get; }

        public ContainerContent(Item item, int amount = 1)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (item is ContainerItem)
            {
                throw new ArgumentException("Container cannot contain another container.", nameof(item));
            }

            if (item is not IStackable && amount != 1)
            {
                throw new ArgumentException("Non-stackable item amount must be 1.", nameof(amount));
            }

            Item = item;
            Amount = amount;
        }
    }
}
