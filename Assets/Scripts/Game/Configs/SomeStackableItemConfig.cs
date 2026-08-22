using Assets.Scripts.InventoryLogic.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game.Configs
{
    [CreateAssetMenu(menuName = "Game/Inventory/Some Stackable Item")]
    public sealed class SomeStackableItemConfig : ItemConfig
    {
        [SerializeField] private int _maxStack = 10;

        public override Item CreateItem()
        {
            return new SomeStackableItem(
                Id,
                _maxStack);
        }

        public override string GetDescription()
        {
            return $"{DisplayName} [{Id}] | Max stack: {_maxStack}";
        }
    }
}
