using Assets.Scripts.Game.Configs;
using Assets.Scripts.InventoryLogic.Inventory;
using Assets.Scripts.InventoryLogic.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Game.UI
{
    public sealed class InventoryPresenter
    {
        private readonly Inventory _inventory;
        private readonly InventoryView _view;
        private readonly ItemDatabase _database;

        private int _selectedSlot = -1;

        public InventoryPresenter(
            Inventory inventory,
            InventoryView view,
            ItemDatabase database)
        {
            _inventory = inventory;
            _view = view;
            _database = database;
            _inventory.StateChanged += Refresh;
        }

        public void Initialize()
        {
            _view.Initialize(_inventory.Capacity);

            _view.SlotClicked += OnSlotClicked;
            _view.SlotDoubleClicked += TryOpenContainer;
            _view.UndoClicked += OnUndoClicked;

            Refresh();
        }

        private void OnSlotClicked(int slotIndex)
        {
            if (_selectedSlot == -1) 
            { 
                _selectedSlot = slotIndex;
                _view.SetSlotSelected(slotIndex,true);
                return;
            }
            var entry = _inventory.GetSlot(_selectedSlot);
            var result=_inventory.Move(_selectedSlot,slotIndex,entry.Amount);
            ResetSelection();
            HandleResult(result);
        }

        private void ResetSelection() 
        {
            if (_selectedSlot == -1)
                return;
            _view.SetSlotSelected(_selectedSlot, false);
            _selectedSlot = -1;
        }

        private void TryOpenContainer(int slotIndex)
        {
            ResetSelection();
            var entry = _inventory.GetSlot(slotIndex);

            if (entry == null)
            {
                _view.ShowError("Slot is empty.");
                return;
            }

            if (entry.Item is not ContainerItem)
            {
                _view.ShowError("This item cannot be opened.");

                return;
            }

            var result = _inventory.OpenContainer(slotIndex);

            HandleResult(result);
        }

        private void OnUndoClicked()
        {
            if (!_inventory.Undo())
            {
                _view.ShowError("Nothing to undo.");
                return;
            }

            _view.ClearError();
            Refresh();
        }

        private void HandleResult(InventoryResult result)
        {
            if (!result.Success)
            {
                _view.ShowError(GetErrorMessage(result.Error));

                return;
            }

            _view.ClearError();
            Refresh();
        }

        public void Refresh()
        {
            for (var i = 0; i < _inventory.Capacity; i++)
            {
                var entry = _inventory.GetSlot(i);

                if (entry == null)
                {
                    _view.ClearSlot(i);
                    continue;
                }

                var config = _database.Get(entry.Item.Id);

                var amount = entry.Amount > 1 ? entry.Amount.ToString() : string.Empty;

                _view.SetSlot(
                    i,
                    config.Icon,
                    amount);
            }

            _view.SetUndoAvailable(_inventory.CanUndo);
        }

        private static string GetErrorMessage(
            InventoryError error)
        {
            return error switch
            {
                InventoryError.NotEnoughSpace =>
                    "Not enough inventory space.",

                InventoryError.StackIsFull =>
                    "Stack is full.",

                InventoryError.ContainerExpected =>
                    "This item cannot be opened.",

                InventoryError.NotEnoughItems =>
                    "Not enough items.",

                InventoryError.InvalidAmount =>
                    "Invalid item amount.",

                _ =>
                    "Inventory operation failed."
            };
        }
    }
}
