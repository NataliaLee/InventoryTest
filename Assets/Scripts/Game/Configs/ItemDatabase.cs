using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game.Configs
{
    [CreateAssetMenu(menuName = "Game/Inventory/Item Database")]
    public sealed class ItemDatabase : ScriptableObject
    {
        [SerializeField]
        private ItemConfig[] _items;

        private Dictionary<string, ItemConfig> _byId;

        public IReadOnlyList<ItemConfig> Items => _items;

        public void Initialize()
        {
            _byId = _items.ToDictionary(x => x.Id);
        }

        public ItemConfig Get(string id)
        {
            return _byId[id];
        }
    }
}
