using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Inventory
{
    internal sealed class InventoryHistory
    {
        private readonly int _capacity;
        private readonly LinkedList<InventoryState> _states = new();

        public InventoryHistory(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
        }

        public void Push(InventoryState state)
        {
            _states.AddLast(state);

            if (_states.Count > _capacity)
                _states.RemoveFirst();
        }

        public bool TryPop(out InventoryState state)
        {
            if (_states.Count == 0)
            {
                state = null!;
                return false;
            }

            state = _states.Last!.Value;
            _states.RemoveLast();

            return true;
        }
    }
}
