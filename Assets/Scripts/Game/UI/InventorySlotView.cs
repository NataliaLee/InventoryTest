using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI
{
    public sealed class InventorySlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private Image _selectionImage;

        public event Action Clicked;
        public event Action DoubleClicked;

        public void Show(Sprite icon, string amount)
        {
            _icon.enabled = true;
            _icon.sprite = icon;

            _amount.text = amount;
        }

        public void SetSelected(bool selected) 
        {
            _selectionImage.gameObject.SetActive(selected);
        }

        public void Clear()
        {
            _icon.enabled = false;
            _amount.text = string.Empty;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (eventData.clickCount == 2)
            {
                DoubleClicked?.Invoke();
                return;
            }

            Clicked?.Invoke();
        }
    }
}
