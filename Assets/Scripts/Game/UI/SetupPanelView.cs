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
    public class SetupPanelView : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _capacityInput;
        [SerializeField]
        private int _maxCapacity = 16;
        [SerializeField]
        private Button _setupBtn;

        public Action<int> OnSetupClicked;

        private void Awake()
        {
            _setupBtn.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (!int.TryParse(_capacityInput.text, out var amount) || amount < 1 || amount>16)
            {
                return;
            }
            OnSetupClicked?.Invoke(amount);
        }
    }
}
