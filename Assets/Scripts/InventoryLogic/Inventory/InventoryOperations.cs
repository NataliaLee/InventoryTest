using Assets.Scripts.InventoryLogic.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Inventory
{
    internal static class InventoryOperations
    {
        public static InventoryResult Add(InventoryState state, Item item, int amount)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (amount <= 0)
                return InventoryResult.Fail(InventoryError.InvalidAmount);

            if (item is IStackable stackable)
            {
                return AddStackable(
                    state,
                    item,
                    stackable,
                    amount);
            }

            return amount == 1
                ? AddSingle(state, item)
                : InventoryResult.Fail(InventoryError.InvalidAmount);
        }

        private static InventoryResult AddSingle(InventoryState state, Item item)
        {
            var slot = state.FindEmptySlot();

            if (slot < 0)
                return InventoryResult.Fail(InventoryError.NotEnoughSpace);

            state.SetSlot(slot, new InventoryEntry(item, 1));
            return InventoryResult.Ok(1);
        }

        private static InventoryResult AddStackable(InventoryState state, Item item, IStackable stackable, int amount)
        {
            if (GetAvailableSpace(state, item, stackable) < amount)
            {
                return InventoryResult.Fail(InventoryError.NotEnoughSpace);
            }

            var remaining = amount;

            // First fill existing incomplete stacks.
            for (var i = 0; i < state.Capacity && remaining > 0; i++)
            {
                var entry = state.GetSlot(i);

                if (entry == null || !IsSameItem(entry.Item, item))
                {
                    continue;
                }

                var freeSpace = stackable.MaxStack - entry.Amount;

                if (freeSpace <= 0)
                    continue;

                var added = Math.Min(remaining, freeSpace);

                entry.Amount += added;
                remaining -= added;
            }

            // Then create new stacks.
            for (var i = 0; i < state.Capacity && remaining > 0; i++)
            {
                if (state.GetSlot(i) != null)
                    continue;

                var stackSize = Math.Min(remaining, stackable.MaxStack);

                state.SetSlot(i, 
                    new InventoryEntry(
                        item,
                        stackSize));

                remaining -= stackSize;
            }

            if (remaining != 0)
            {
                throw new InvalidOperationException("Inventory capacity invariant violated.");
            }

            return InventoryResult.Ok(amount);
        }

        public static InventoryResult Remove(InventoryState state, Item item, int amount)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (amount <= 0)
                return InventoryResult.Fail(InventoryError.InvalidAmount);

            if (Count(state, item) < amount)
            {
                return InventoryResult.Fail(InventoryError.NotEnoughItems);
            }

            var remaining = amount;

            for (var i = state.Capacity - 1; i >= 0 && remaining > 0; i--)
            {
                var entry = state.GetSlot(i);

                if (entry == null || !IsSameItem(entry.Item, item))
                {
                    continue;
                }

                var removed = Math.Min(entry.Amount, remaining);

                entry.Amount -= removed;
                remaining -= removed;

                if (entry.Amount == 0)
                    state.SetSlot(i, null);
            }

            if (remaining != 0)
            {
                throw new InvalidOperationException("Inventory remove invariant violated.");
            }

            return InventoryResult.Ok(amount);
        }

        public static InventoryResult RemoveAt(InventoryState state,int slot, int amount)
        {
            if (!IsValidSlot(state, slot))
                return InventoryResult.Fail(InventoryError.InvalidSlot);

            if (amount <= 0)
                return InventoryResult.Fail(InventoryError.InvalidAmount);

            var entry = state.GetSlot(slot);

            if (entry == null)
                return InventoryResult.Fail(InventoryError.SlotIsEmpty);

            if (amount > entry.Amount)
            {
                return InventoryResult.Fail(InventoryError.NotEnoughItems);
            }

            // A non-stackable entry cannot be partially removed.
            if (entry.Item is not IStackable && amount != entry.Amount)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            entry.Amount -= amount;

            if (entry.Amount == 0)
                state.SetSlot(slot, null);

            return InventoryResult.Ok(amount);
        }

        public static InventoryResult Move(InventoryState state, int sourceSlot, int targetSlot, int amount)
        {
            if (!IsValidSlot(state, sourceSlot) || !IsValidSlot(state, targetSlot))
            {
                return InventoryResult.Fail(InventoryError.InvalidSlot);
            }

            if (amount <= 0)
                return InventoryResult.Fail(InventoryError.InvalidAmount);

            if (sourceSlot == targetSlot)
                return InventoryResult.Ok(0);

            var source = state.GetSlot(sourceSlot);

            if (source == null)
                return InventoryResult.Fail(InventoryError.SlotIsEmpty);

            if (amount > source.Amount)
            {
                return InventoryResult.Fail(InventoryError.NotEnoughItems);
            }

            var target = state.GetSlot(targetSlot);

            // Empty target -> move or split.
            if (target == null)
            {
                return MoveToEmptySlot(
                    state,
                    sourceSlot,
                    targetSlot,
                    source,
                    amount);
            }

            // Same stackable item -> merge.
            if (IsSameItem(source.Item, target.Item) &&
                source.Item is IStackable stackable)
            {
                return Merge(
                    state,
                    sourceSlot,
                    source,
                    target,
                    stackable,
                    amount);
            }

            // Different/non-stackable items -> full swap only.
            if (amount != source.Amount)
            {
                return InventoryResult.Fail(InventoryError.SwapIsNotAllowed);
            }

            state.SetSlot(sourceSlot, target);
            state.SetSlot(targetSlot, source);

            return InventoryResult.Ok(amount);
        }

        private static InventoryResult MoveToEmptySlot(
            InventoryState state,
            int sourceSlot,
            int targetSlot,
            InventoryEntry source,
            int amount)
        {
            // Moving the whole entry.
            if (amount == source.Amount)
            {
                state.SetSlot(targetSlot, source);
                state.SetSlot(sourceSlot, null);

                return InventoryResult.Ok(amount);
            }

            // Partial move requires stackability.
            if (source.Item is ContainerItem)
            {
                return InventoryResult.Fail(InventoryError.InvalidAmount);
            }

            source.Amount -= amount;

            state.SetSlot(
                targetSlot,
                new InventoryEntry(
                    source.Item,
                    amount));

            return InventoryResult.Ok(amount);
        }

        private static InventoryResult Merge(
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

            // Partial merge is explicitly allowed.
            var moved = Math.Min(requestedAmount, freeSpace);

            source.Amount -= moved;
            target.Amount += moved;

            if (source.Amount == 0)
                state.SetSlot(sourceSlot, null);

            return InventoryResult.Ok(moved);
        }

        public static InventoryResult OpenContainer(InventoryState state, int slot)
        {
            if (!IsValidSlot(state, slot))
                return InventoryResult.Fail(InventoryError.InvalidSlot);

            var entry = state.GetSlot(slot);

            if (entry == null)
                return InventoryResult.Fail(InventoryError.SlotIsEmpty);

            if (entry.Item is not ContainerItem container)
            {
                return InventoryResult.Fail(InventoryError.ContainerExpected);
            }

            state.SetSlot(slot, null);

            foreach (var content in container.Contents)
            {
                if (content.Item is ContainerItem)
                {
                    return InventoryResult.Fail(InventoryError.ContainerInsideContainer);
                }

                var result = Add(
                    state,
                    content.Item,
                    content.Amount);

                if (!result.Success)
                    return result;
            }

            return InventoryResult.Ok(1);
        }

        private static long GetAvailableSpace(InventoryState state, Item item, IStackable stackable)
        {
            long available = 0;

            for (var i = 0; i < state.Capacity; i++)
            {
                var entry = state.GetSlot(i);

                if (entry == null)
                {
                    available += stackable.MaxStack;
                    continue;
                }

                if (!IsSameItem(entry.Item, item))
                    continue;

                available += stackable.MaxStack - entry.Amount;
            }

            return available;
        }

        private static int Count(InventoryState state, Item item)
        {
            var count = 0;

            for (var i = 0; i < state.Capacity; i++)
            {
                var entry = state.GetSlot(i);

                if (entry == null || !IsSameItem(entry.Item, item))
                {
                    continue;
                }

                checked
                {
                    count += entry.Amount;
                }
            }

            return count;
        }

        private static bool IsSameItem(Item first, Item second)
        {
            return string.Equals(first.Id, second.Id, StringComparison.Ordinal);
        }

        private static bool IsValidSlot(InventoryState state, int slot)
        {
            return slot >= 0 && slot < state.Capacity;
        }
    }
}
