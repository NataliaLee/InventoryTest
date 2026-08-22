using Assets.Scripts.Game.Configs;
using Assets.Scripts.Game.UI;
using Assets.Scripts.InventoryLogic.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Game
{
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private int _historyCapacity = 3;
        [SerializeField] private InventoryView _view;
        [SerializeField] private ControlPanelView _controlPanelView;
        [SerializeField] private SetupPanelView _setupPanelView;
        [SerializeField] private ItemDatabase _database;

        [SerializeField] private ItemConfig[] _startingItems;

        private Inventory _inventory;
        private InventoryPresenter _inventoryPresenter;
        private ControlPanelPresenter _controlPresenter;

        private void Awake()
        {
            _database.Initialize();
            _setupPanelView.OnSetupClicked += Initialize;
        }


        private void Initialize(int capacity)
        {
            _setupPanelView.OnSetupClicked -= Initialize;
            _inventory = new Inventory(capacity, _historyCapacity);

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
            _setupPanelView.gameObject.SetActive(false);
        }
    }
}
