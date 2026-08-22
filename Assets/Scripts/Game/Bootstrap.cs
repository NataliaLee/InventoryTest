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
        [SerializeField] private int _historyCapacity = 3;
        [SerializeField] private InventoryView _view;
        [SerializeField] private ControlPanelView _controlPanelView;
        [SerializeField] private ItemDatabase _database;

        [SerializeField] private ItemConfig[] _startingItems;

        private Inventory _inventory;
        private InventoryPresenter _inventoryPresenter;
        private ControlPanelPresenter _controlPresenter;

        private void Awake()
        {
            _database.Initialize();

            _inventory = new Inventory(_inventoryCapacity, _historyCapacity);

            foreach (var config in _startingItems)
            {
                _inventory.Add(config.CreateItem());
            }

            _inventoryPresenter = new InventoryPresenter(
                _inventory,
                _view,
                _database);

            _inventoryPresenter.Initialize();
            _controlPresenter = new ControlPanelPresenter(
                _inventory,
                _database,
                _controlPanelView
                );
            _controlPresenter.Initialize();
        }
    }
}
