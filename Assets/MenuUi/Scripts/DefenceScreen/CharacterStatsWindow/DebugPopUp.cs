using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Signals;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Controls character stats window debug popup functionality.
    /// </summary>
    public class DebugPopUp : AltMonoBehaviour
    {
        
        [SerializeField] private GameObject _contents;
        [SerializeField] private Image _touchBlocker;
        [SerializeField] private Toggle _unlimitedUpgradeMaterialsToggle;
        [SerializeField] private Button _addUpgradeMaterialsButton;

        private StatsWindowController _controller;

        private const int UpgradeMaterialsToAdd = 10000;


        private void OnDestroy()
        {
            _unlimitedUpgradeMaterialsToggle.onValueChanged.RemoveAllListeners();
            _addUpgradeMaterialsButton.onClick.RemoveAllListeners();
        }


        /// <summary>
        /// Open DebugPopUp
        /// </summary>
        public void OpenPopUp()
        {
            // If _controller is null initializing
            if (_controller == null)
            {
                _controller = FindObjectOfType<StatsWindowController>();

                _unlimitedUpgradeMaterialsToggle.isOn = SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials;
                _addUpgradeMaterialsButton.interactable = !SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials;

                _unlimitedUpgradeMaterialsToggle.onValueChanged.AddListener(value =>
                {
                    SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials = value;
                    _addUpgradeMaterialsButton.interactable = !value;
                });

                _addUpgradeMaterialsButton.onClick.AddListener(AddUpgradeMaterials);
            }

            // Setting popup active
            _contents.SetActive(true);
            _touchBlocker.enabled = true;
        }


        /// <summary>
        /// Close DebugPopUp
        /// </summary>
        public void ClosePopUp()
        {
            _touchBlocker.enabled = false;
            _contents.SetActive(false);
        }


        private void AddUpgradeMaterials()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                playerData.DiamondSpeed += UpgradeMaterialsToAdd;
                _controller.InvokeOnUpgradeMaterialAmountChanged();
            }));
        }
    }
}
