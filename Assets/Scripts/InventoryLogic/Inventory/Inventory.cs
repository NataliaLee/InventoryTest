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
        private readonly int _historyCapacity;
        private readonly LinkedList<InventoryState> _history = new();

        private InventoryState _state;
        public int Capacity => _state.Slots.Length;
        public int UndoCount => _history.Count;

        public Inventory(int capacity, int historyCapacity = 3)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _state = new InventoryState(capacity);
            _historyCapacity = historyCapacity;
        }

        public InventoryEntry? GetSlot(int index)
        {
            if (!IsValidSlot(index))
                throw new ArgumentOutOfRangeException(nameof(index));
            return _state.Slots[index];
        }

        public InventoryResult Add(Item item, int amount = 1)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (amount <= 0)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            return ExecuteAtomic(
                state => AddInternal(
                    state,
                    item,
                    amount));
        }

        public InventoryResult Remove(Item item, int amount = 1)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (amount <= 0)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            return ExecuteAtomic(
                state => RemoveInternal(
                    state,
                    item,
                    amount));
        }

        public InventoryResult RemoveAt(int slotIndex, int amount = 1)
        {
            if (!IsValidSlot(slotIndex))
            {
                return InventoryResult.Fail(InventoryError.InvalidSlot);
            }

            if (amount <= 0)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            return ExecuteAtomic(
                state => RemoveAtInternal(
                    state,
                    slotIndex,
                    amount));
        }

        public InventoryResult Move(int sourceSlot, int targetSlot, int amount)
        {
            if (!IsValidSlot(sourceSlot) ||
                !IsValidSlot(targetSlot))
            {
                return InventoryResult.Fail(InventoryError.InvalidSlot);
            }

            if (amount <= 0)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            if (sourceSlot == targetSlot)
                return InventoryResult.Ok(0);

            return ExecuteAtomic(
                state => MoveInternal(
                    state,
                    sourceSlot,
                    targetSlot,
                    amount));
        }

        public InventoryResult MoveAll(int sourceSlot, int targetSlot)
        {
            if (!IsValidSlot(sourceSlot) ||
                !IsValidSlot(targetSlot))
            {
                return InventoryResult.Fail(InventoryError.InvalidSlot);
            }

            var source = _state.Slots[sourceSlot];

            if (source == null)
            {
                return InventoryResult.Fail(InventoryError.SlotIsEmpty);
            }

            return Move( 
                sourceSlot,
                targetSlot,
                source.Amount);
        }

        public InventoryResult OpenContainer(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return InventoryResult.Fail(InventoryError.InvalidSlot);
            }

            return ExecuteAtomic(
                state => OpenContainerInternal(
                    state,
                    slotIndex));
        }

        public bool Undo()
        {
            if (_history.Count == 0)
                return false;

            _state = _history.Last!.Value;

            _history.RemoveLast();

            return true;
        }

        private InventoryResult ExecuteAtomic(Func<InventoryState, InventoryResult> operation)
        {
            var workingState = _state.Clone();

            var result = operation(workingState);

            if (!result.Success)
                return result;

            if (result.AffectedAmount == 0)
                return result;

            PushHistory(_state);

            _state = workingState;

            return result;
        }

        private void PushHistory(InventoryState state)
        {
            _history.AddLast(state);

            while (_history.Count > _historyCapacity)
                _history.RemoveFirst();
        }

        private static InventoryResult AddInternal(InventoryState state, Item item, int amount)
        {
            if (item is IStackable stackable)
            {
                return AddStackableInternal(
                    state,
                    item,
                    stackable,
                    amount);
            }

            if (amount != 1)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            return AddSingleInternal(
                state,
                item);
        }

        private static InventoryResult AddStackableInternal(InventoryState state, Item item, IStackable stackable, int amount)
        {
            var availableSpace = CalculateAvailableSpace( state, item, stackable);

            if (availableSpace < amount)
            {
                return InventoryResult.Fail(InventoryError.NotEnoughSpace);
            }

            var remaining = amount;

            // Fill existing incomplete stacks first.
            foreach (var entry in state.Slots)
            {
                if (remaining == 0)
                    break;

                if (entry == null)
                    continue;

                if (!IsSameItem(entry.Item, item))
                    continue;

                var freeSpace =
                    stackable.MaxStack - entry.Amount;

                if (freeSpace <= 0)
                    continue;

                var amountToAdd = Math.Min(remaining, freeSpace);

                entry.Amount += amountToAdd;
                remaining -= amountToAdd;
            }

            // Then use empty slots.
            for (var i = 0;i < state.Slots.Length && remaining > 0;i++)
            {
                if (state.Slots[i] != null)
                    continue;

                var stackSize = Math.Min(remaining, stackable.MaxStack);
                state.Slots[i] = new InventoryEntry(item, stackSize);
                remaining -= stackSize;
            }

            if (remaining != 0)
            {
                throw new InvalidOperationException("Inventory add invariant violated.");
            }

            return InventoryResult.Ok(amount);
        }

        private static InventoryResult AddSingleInternal(InventoryState state, Item item)
        {
            for (var i = 0; i < state.Slots.Length; i++)
            {
                if (state.Slots[i] != null)
                    continue;

                state.Slots[i] = new InventoryEntry(item, 1);
                return InventoryResult.Ok(1);
            }

            return InventoryResult.Fail(InventoryError.NotEnoughSpace);
        }

        private static InventoryResult RemoveInternal(InventoryState state, Item item, int amount)
        {
            if (Count(state, item) < amount)
            {
                return InventoryResult.Fail(InventoryError.NotEnoughItems);
            }

            var remaining = amount;

            for (var i = state.Slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                var entry = state.Slots[i];

                if (entry == null)
                    continue;

                if (!IsSameItem(entry.Item, item))
                    continue;

                var amountToRemove = Math.Min(entry.Amount, remaining);

                entry.Amount -= amountToRemove;
                remaining -= amountToRemove;

                if (entry.Amount == 0)
                    state.Slots[i] = null;
            }

            return InventoryResult.Ok(amount);
        }

        private static InventoryResult RemoveAtInternal(InventoryState state, int slotIndex, int amount)
        {
            var entry = state.Slots[slotIndex];

            if (entry == null)
            {
                return InventoryResult.Fail(InventoryError.SlotIsEmpty);
            }

            if (amount > entry.Amount)
            {
                return InventoryResult.Fail(InventoryError.NotEnoughItems);
            }

            if (entry.Item is not IStackable && amount != entry.Amount)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            entry.Amount -= amount;

            if (entry.Amount == 0)
                state.Slots[slotIndex] = null;

            return InventoryResult.Ok(amount);
        }

        private static InventoryResult MoveInternal(InventoryState state, int sourceSlot, int targetSlot, int requestedAmount)
        {
            var source = state.Slots[sourceSlot];

            if (source == null)
            {
                return InventoryResult.Fail(InventoryError.SlotIsEmpty);
            }

            if (requestedAmount > source.Amount)
            {
                return InventoryResult.Fail(InventoryError.NotEnoughItems);
            }

            var target = state.Slots[targetSlot];

            // Empty destination.
            if (target == null)
            {
                return MoveToEmptySlot(
                    state,
                    sourceSlot,
                    targetSlot,
                    source,
                    requestedAmount);
            }

            // Same item + stackable => merge.
            if (IsSameItem(source.Item,target.Item))
            {
                if (source.Item is not IStackable stackable)
                {
                    return InventoryResult.Fail(InventoryError.StackIsFull);
                }

                return MergeStacks(
                    state,
                    sourceSlot,
                    source,
                    target,
                    stackable,
                    requestedAmount);
            }

            // Different items => full swap only.
            if (requestedAmount != source.Amount)
            {
                return InventoryResult.Fail(InventoryError.SwapIsNotAllowed);
            }

            state.Slots[sourceSlot] = target;
            state.Slots[targetSlot] = source;

            return InventoryResult.Ok(requestedAmount);
        }

        private static InventoryResult MoveToEmptySlot(
            InventoryState state,
            int sourceSlot,
            int targetSlot,
            InventoryEntry source,
            int amount)
        {
            // Full entry move.
            if (amount == source.Amount)
            {
                state.Slots[targetSlot] = source;
                state.Slots[sourceSlot] = null;

                return InventoryResult.Ok(amount);
            }

            // Only stackable items may be split.
            if (source.Item is not IStackable)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            source.Amount -= amount;
            state.Slots[targetSlot] = new InventoryEntry(source.Item, amount);

            return InventoryResult.Ok(amount);
        }

        private static InventoryResult MergeStacks(
            InventoryState state,
            int sourceSlot,
            InventoryEntry source,
            InventoryEntry target,
            IStackable stackable,
            int requestedAmount)
        {
            var freeSpace = stackable.MaxStack - target.Amount;

            if (freeSpace <= 0)
            {
                return InventoryResult.Fail(InventoryError.StackIsFull);
            }

            var amountToMove = Math.Min(requestedAmount, freeSpace);

            target.Amount += amountToMove;
            source.Amount -= amountToMove;

            if (source.Amount == 0)
                state.Slots[sourceSlot] = null;

            return InventoryResult.Ok(amountToMove);
        }

        private static InventoryResult OpenContainerInternal(InventoryState state, int slotIndex)
        {
            var entry = state.Slots[slotIndex];

            if (entry == null)
            {
                return InventoryResult.Fail(InventoryError.SlotIsEmpty);
            }

            if (entry.Item is not IContainer container)
            {
                return InventoryResult.Fail(InventoryError.ContainerExpected);
            }

            state.Slots[slotIndex] = null;

            foreach (var content in container.Contents)
            {
                var result = AddInternal(
                    state,
                    content.Item,
                    content.Amount);

                if (!result.Success)
                    return result;
            }

            return InventoryResult.Ok(1);
        }

        private static long CalculateAvailableSpace(InventoryState state, Item item, IStackable stackable)
        {
            long result = 0;

            foreach (var entry in state.Slots)
            {
                if (entry == null)
                {
                    result += stackable.MaxStack;
                    continue;
                }

                if (!IsSameItem(entry.Item, item))
                {
                    continue;
                }

                result += stackable.MaxStack - entry.Amount;
            }

            return result;
        }

        private static int Count(InventoryState state, Item item)
        {
            var result = 0;

            foreach (var entry in state.Slots)
            {
                if (entry == null)
                    continue;

                if (!IsSameItem(entry.Item, item))
                {
                    continue;
                }
                result += entry.Amount;
            }

            return result;
        }

        private static bool IsSameItem(Item lhs, Item rhs)
        {
            return string.Equals(lhs.Id, rhs.Id, StringComparison.Ordinal);
        }

        private bool IsValidSlot(int index)
        {
            return index >= 0 && index < _state.Slots.Length;
        }
    }
}
