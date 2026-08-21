using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Items
{
    public abstract class Item
    {
        public string Id { get; }
        protected Item(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Item id cannot be empty.", nameof(id));

            Id = id;
        }
    }
}
