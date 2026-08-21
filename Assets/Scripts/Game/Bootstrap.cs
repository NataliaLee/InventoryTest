using Assets.Scripts.Game.Configs;
using Assets.Scripts.Game.UI;
using Assets.Scripts.InventoryLogic.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private int _inventoryCapacity;
        [SerializeField] private InventoryView _view;
        [SerializeField] private ItemDatabase _database;

        [SerializeField] private ItemConfig[] _startingItems;

        private Inventory _inventory;
        private InventoryPresenter _presenter;

        private void Awake()
        {
            _database.Initialize();

            _inventory = new Inventory(_inventoryCapacity);

            foreach (var config in _startingItems)
            {
                _inventory.Add(config.CreateItem());
            }

            _presenter = new InventoryPresenter(
                _inventory,
                _view,
                _database);

            _presenter.Initialize();
        }
    }
}
