using Assets.Scripts.Game.Configs;
using Assets.Scripts.InventoryLogic.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Game.UI
{
    public sealed class ControlPanelPresenter
    {
        private readonly Inventory _inventory;
        private readonly ItemDatabase _database;
        private readonly ControlPanelView _view;

        public ControlPanelPresenter(
            Inventory inventory,
            ItemDatabase database,
            ControlPanelView view)
        {
            _inventory = inventory;
            _database = database;
            _view = view;
        }

        public void Initialize()
        {
            _view.SetItems(_database.Items);
            _view.AddClicked += OnAddClicked;
        }

        private void OnAddClicked(int itemIndex, int amount)
        {
            var config = _database.Items[itemIndex];

            var item = config.CreateItem();

            var result = _inventory.Add(item, amount);

            if (!result.Success)
            {
                _view.ShowResult(GetErrorMessage(result.Error));
                return;
            }

            _view.ShowResult($"Added {config.DisplayName} x{result.AffectedAmount}");
        }

        private static string GetErrorMessage(InventoryError error)
        {
            return error switch
            {
                InventoryError.NotEnoughSpace => "Not enough inventory space.",

                InventoryError.InvalidAmount => "Invalid amount.",

                _ => $"Inventory error: {error}"
            };
        }
    }
}
