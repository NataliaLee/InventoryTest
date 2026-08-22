using Assets.Scripts.InventoryLogic.Items;
using System;
using System.Text;
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
        public override string GetDescription()
        {
            var builder = new StringBuilder();

            builder.Append(DisplayName)
                .Append(" [")
                .Append(Id)
                .Append("] | Container");

            if (_contents.Length == 0)
                return builder.Append(": empty").ToString();

            builder.Append(": ");

            for (var i = 0; i < _contents.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(_contents[i].Item.DisplayName)
                    .Append(" x")
                    .Append(_contents[i].Amount);
            }

            return builder.ToString();
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
