using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Inventory
{
    internal sealed class InventoryState
    {
        private readonly InventoryEntry?[] _slots;
        public int Capacity => _slots.Length;
        public InventoryEntry? GetSlot(int index) => _slots[index];
        public IEnumerable<InventoryEntry?> Slots => _slots;

        public InventoryState(int capacity)
        {
            _slots = new InventoryEntry?[capacity];
        }

        private InventoryState(InventoryEntry?[] slots)
        {
            _slots = slots;
        }

        public void SetSlot(int index, InventoryEntry? entry)
        {
            _slots[index] = entry;
        }


        public int FindEmptySlot()
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null)
                    return i;
            }

            return -1;
        }

        public InventoryState Clone()
        {
            var copy = new InventoryEntry?[_slots.Length];

            for (var i = 0; i < _slots.Length; i++)
                copy[i] = _slots[i]?.Clone();

            return new InventoryState(copy);
        }
    }
}
