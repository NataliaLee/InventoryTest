using Assets.Scripts.InventoryLogic.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Inventory
{
    public sealed class Inventory
    {
        private readonly InventoryHistory _history;
        private InventoryState _state;
        public int Capacity => _state.Capacity;

        public bool CanUndo => _history.CanUndo;
        
        public event Action StateChanged;

        public Inventory(int capacity, int historyCapacity = 10)
        {
            _state = new InventoryState(capacity);
            _history = new InventoryHistory(historyCapacity);
        }

        public InventoryEntry? GetSlot(int index) => _state.GetSlot(index);

        public InventoryResult Add(Item item, int amount = 1)
        {
            return Execute(
                state => InventoryOperations.Add(
                    state,
                    item,
                    amount));
        }

        public InventoryResult Remove(Item item, int amount = 1)
        {
            return Execute(
                state => InventoryOperations.Remove(
                    state,
                    item,
                    amount));
        }

        public InventoryResult Move(int from, int to, int amount)
        {
            return Execute(
                state => InventoryOperations.Move(
                    state,
                    from,
                    to,
                    amount));
        }

        public InventoryResult OpenContainer(int slot)
        {
            return Execute(
                state => InventoryOperations.OpenContainer(
                    state,
                    slot));
        }

        public bool Undo()
        {
            if (!_history.TryPop(out var state))
                return false;

            _state = state;
            return true;
        }

        private InventoryResult Execute(Func<InventoryState, InventoryResult> operation)
        {
            var workingState = _state.Clone();

            var result = operation(workingState);

            if (!result.Success || result.AffectedAmount == 0)
                return result;

            _history.Push(_state);
            _state = workingState;
            StateChanged?.Invoke();
            return result;
        }
    }

}
