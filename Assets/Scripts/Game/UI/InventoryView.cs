using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{
    public sealed class InventoryView : MonoBehaviour
    {
        [SerializeField]
        private Transform _slotsRoot;

        [SerializeField]
        private InventorySlotView _slotPrefab;

        [SerializeField]
        private Button _undoButton;

        [SerializeField]
        private TMP_Text _errorText;

        private readonly List<InventorySlotView> _slots = new();

        public event Action<int> SlotClicked;
        public event Action<int> SlotDoubleClicked;
        public event Action UndoClicked;

        public void Initialize(int capacity)
        {
            for (var i = 0; i < capacity; i++)
            {
                var slot = Instantiate(
                    _slotPrefab,
                    _slotsRoot);

                var index = i;

                slot.Clicked += () =>  SlotClicked?.Invoke(index);
                slot.DoubleClicked += () => SlotDoubleClicked?.Invoke(index);

                _slots.Add(slot);
            }
            ClearError();
            _undoButton.onClick.AddListener(() => UndoClicked?.Invoke());
        }

        public void SetSlotSelected(int index, bool selected) 
        {
            _slots[index].SetSelected(selected);
        }

        public void SetSlot(
            int index,
            Sprite icon,
            string amount)
        {
            _slots[index].Show(
                icon,
                amount);
        }

        public void ClearSlot(int index)
        {
            _slots[index].Clear();
        }

        public void ShowError(string message)
        {
            _errorText.text = message;
        }

        public void ClearError()
        {
            _errorText.text = string.Empty;
        }

        public void SetUndoAvailable(bool available)
        {
            _undoButton.interactable = available;
        }
    }
}
