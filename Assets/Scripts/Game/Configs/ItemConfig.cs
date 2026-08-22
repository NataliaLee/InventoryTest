using Assets.Scripts.InventoryLogic.Items;
using UnityEngine;

namespace Assets.Scripts.Game.Configs
{
    public abstract class ItemConfig : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;

        public abstract Item CreateItem();
        public virtual string GetDescription()
        {
            return $"{DisplayName} [{Id}]";
        }
    }
}
