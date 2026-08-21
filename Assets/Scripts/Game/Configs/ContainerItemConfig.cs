using Assets.Scripts.InventoryLogic.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game.Configs
{
    [CreateAssetMenu(menuName = "Game/Inventory/Container")]
    public sealed class ContainerItemConfig : ItemConfig
    {
        [SerializeField]
        private ContainerContentConfig[] _contents;

        public override Item CreateItem()
        {
            var contents = new ContainerContent[_contents.Length];

            for (var i = 0; i < _contents.Length; i++)
            {
                var content = _contents[i];

                contents[i] = new ContainerContent(
                    content.Item.CreateItem(),
                    content.Amount);
            }

            return new ContainerItem(Id, contents);
        }
    }

    [Serializable]
    public sealed class ContainerContentConfig
    {
        [SerializeField] private ItemConfig _item;
        [SerializeField] private int _amount = 1;

        public ItemConfig Item => _item;
        public int Amount => _amount;
    }
}
