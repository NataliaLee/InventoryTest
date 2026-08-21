using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Items
{
    public sealed class SomeStackableItem : Item, IStackable
    {
        public int MaxStack { get; }
        public SomeStackableItem(string id, int maxStack) : base(id)
        {
            if (maxStack <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxStack));

            MaxStack = maxStack;
        }

    }

}
