using Assets.Scripts.Game.Configs;
using Assets.Scripts.InventoryLogic.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{
    public class ControlPanelView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown _itemDropdown;
        [SerializeField]
        private TMP_InputField _amountInput;
        [SerializeField]
        private TMP_Text _itemDescription;
        [SerializeField]
        private Button _addButton;
        [SerializeField] 
        private TMP_Text _resultText;

        private IReadOnlyList<ItemConfig> _items;

        public event Action<int, int> AddClicked;

        private void Awake()
        {
            _addButton.onClick.AddListener(OnAddClicked);
            _itemDropdown.onValueChanged.AddListener(OnItemChose);
        }


        public void SetItems(IReadOnlyList<ItemConfig> items)
        {
            _itemDropdown.ClearOptions();
            _items = items;
            _itemDropdown.AddOptions(
                items
                    .Select(x => x.DisplayName)
                    .ToList());
        }

        public void ShowResult(string message)
        {
            _resultText.text = message;
        }
        private void OnItemChose(int index)
        {
            if (_items?.Count <= index) 
            {
                _amountInput.text = "1";
                _amountInput.enabled = false;
                return;
            }
            var itemConfig = _items[index];
            if(itemConfig is ContainerItemConfig)
            {
                _amountInput.text = "1";
                _amountInput.enabled = false;
            }
            else
            {
                _amountInput.enabled = true;
            }
            _itemDescription.text = itemConfig.GetDescription();
        }

        private void OnAddClicked()
        {
            if (!int.TryParse(_amountInput.text,out var amount))
            {
                ShowResult("Invalid amount.");
                return;
            }

            AddClicked?.Invoke(
                _itemDropdown.value,
                amount);
        }

    }
}
