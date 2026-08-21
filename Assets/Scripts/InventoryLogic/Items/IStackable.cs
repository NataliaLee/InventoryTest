using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Items
{
    public interface IStackable
    {
        int MaxStack { get; }
    }
}
