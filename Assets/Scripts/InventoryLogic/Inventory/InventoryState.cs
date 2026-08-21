using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Inventory
{
    internal sealed class InventoryState
    {
        public InventoryState(int capacity)
        {
            Slots = new InventoryEntry?[capacity];
        }

        private InventoryState(InventoryEntry?[] slots)
        {
            Slots = slots;
        }

        public InventoryEntry?[] Slots { get; }

        public InventoryState Clone()
        {
            var slots = new InventoryEntry?[Slots.Length];

            for (var i = 0; i < Slots.Length; i++)
                slots[i] = Slots[i]?.Clone();

            return new InventoryState(slots);
        }
    }
}
