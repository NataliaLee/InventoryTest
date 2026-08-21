using Assets.Scripts.InventoryLogic.Inventory;
using Assets.Scripts.InventoryLogic.Items;
using NUnit.Framework;
using UnityEngine;
public sealed class InventoryTests
{
    [Test]
    public void Add_DistributesItemsBetweenStacks()
    {
        var potion = new StackableItem("potion", 10);

        var inventory = new Inventory(3);

        var result = inventory.Add(
            potion,
            25);

        Assert.That(result.Success, Is.True);
        Assert.That(inventory.GetSlot(0)!.Amount, Is.EqualTo(10));
        Assert.That(inventory.GetSlot(1)!.Amount, Is.EqualTo(10));
        Assert.That(inventory.GetSlot(2)!.Amount, Is.EqualTo(5));
    }
}
