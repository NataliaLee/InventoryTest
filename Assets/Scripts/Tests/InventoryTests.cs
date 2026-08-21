using Assets.Scripts.InventoryLogic.Inventory;
using Assets.Scripts.InventoryLogic.Items;
using NUnit.Framework;

public sealed class InventoryTests
{
    private TestStackableItem _potion;
    private TestStackableItem _wood;
    private TestStackableItem _stone;

    [SetUp]
    public void SetUp()
    {
        _potion = new TestStackableItem("potion", maxStack: 10);
        _wood = new TestStackableItem("wood", maxStack: 20);
        _stone = new TestStackableItem("stone", maxStack: 20);
    }

    // ============================================================
    // Stack overflow and distribution
    // ============================================================

    [Test]
    public void Add_AmountExceedsMaxStack_DistributesItemsAcrossSlots()
    {
        var inventory = new Inventory(capacity: 3);
        var result = inventory.Add(_potion, 25);
        
        Assert.That(result.Success, Is.True);

        AssertEntry(inventory, 0, _potion, 10);
        AssertEntry(inventory, 1, _potion, 10);
        AssertEntry(inventory, 2, _potion, 5);
    }

    // ============================================================
    // Partial stack merge
    // ============================================================

    [Test]
    public void Move_TargetStackHasNotEnoughSpace_MergesPartially()
    {
        var inventory = new Inventory(capacity: 2);

        inventory.Add(_potion, 15);

        /*
         * Before:
         *
         * slot 0: Potion x10
         * slot 1: Potion x5
         *
         * Target has space only for 5 items.
         */

        var result = inventory.Move(0, 1, amount: 10);

        Assert.That(result.Success, Is.True);
        Assert.That(result.AffectedAmount, Is.EqualTo(5));

        AssertEntry(inventory, 0, _potion, 5);
        AssertEntry(inventory, 1, _potion, 10);
    }

    // ============================================================
    // Failed container opening must not change state
    // ============================================================

    [Test]
    public void OpenContainer_ContentDoesNotFit_PreservesInventoryState()
    {
        var inventory = new Inventory(capacity: 2);

        inventory.Add(_stone, 20);

        var container = new ContainerItem(
            "container",
            new[]
            {
                    new ContainerContent(_wood, 21)
            });

        inventory.Add(container);

        /*
         * Before:
         *
         * slot 0: Stone x20
         * slot 1: Container
         *
         * Opening the container frees one slot.
         * Wood.MaxStack = 20, but container contains 21.
         *
         * The content cannot fit completely.
         */

        var result = inventory.OpenContainer(1);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Error,
            Is.EqualTo(InventoryError.NotEnoughSpace));

        // Original inventory must remain unchanged.
        AssertEntry(inventory, 0, _stone, 20);

        var containerEntry = inventory.GetSlot(1);

        Assert.That(containerEntry, Is.Not.Null);
        Assert.That(containerEntry!.Item, Is.SameAs(container));
        Assert.That(containerEntry.Amount, Is.EqualTo(1));
    }

    // ============================================================
    // Undo
    // ============================================================

    [Test]
    public void Undo_AfterSuccessfulOperation_RestoresPreviousState()
    {
        var inventory = new Inventory(capacity: 3);

        inventory.Add(_potion, 7);

        /*
         * State before operation:
         *
         * slot 0: Potion x7
         * slot 1: Empty
         * slot 2: Empty
         */

        inventory.Add(_potion, 8);

        /*
         * State after operation:
         *
         * slot 0: Potion x10
         * slot 1: Potion x5
         * slot 2: Empty
         */

        AssertEntry(inventory, 0, _potion, 10);
        AssertEntry(inventory, 1, _potion, 5);

        var result = inventory.Undo();

        Assert.That(result, Is.True);

        AssertEntry(inventory, 0, _potion, 7);
        Assert.That(inventory.GetSlot(1), Is.Null);
        Assert.That(inventory.GetSlot(2), Is.Null);
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static void AssertEntry(
        Inventory inventory,
        int slot,
        Item expectedItem,
        int expectedAmount)
    {
        var entry = inventory.GetSlot(slot);

        Assert.That(
            entry,
            Is.Not.Null,
            $"Expected slot {slot} to contain an item.");

        Assert.That(
            entry!.Item,
            Is.SameAs(expectedItem));

        Assert.That(
            entry.Amount,
            Is.EqualTo(expectedAmount));
    }

    private sealed class TestStackableItem : Item, IStackable
    {
        public TestStackableItem(
            string id,
            int maxStack)
            : base(id)
        {
            MaxStack = maxStack;
        }

        public int MaxStack { get; }
    }

}
