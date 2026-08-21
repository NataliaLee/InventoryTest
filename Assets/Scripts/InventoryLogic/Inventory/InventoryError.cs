

namespace Assets.Scripts.InventoryLogic.Inventory
{
    public enum InventoryError
    {
        None = 0,

        InvalidAmount,
        InvalidSlot,

        SlotIsEmpty,

        NotEnoughItems,
        NotEnoughSpace,

        StackIsFull,

        SwapIsNotAllowed,

        ContainerExpected,
        ContainerInsideContainer
    }
}
