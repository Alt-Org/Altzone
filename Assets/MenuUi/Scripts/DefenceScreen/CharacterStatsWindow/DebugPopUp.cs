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
        [SerializeField] private Toggle _debugToggle;
        [SerializeField] private Button _addUpgradeMaterialsButton;

        private StatsWindowController _controller;

        private const int UpgradeMaterialsToAdd = 10000;


        private void Awake()
        {
            _controller = FindObjectOfType<StatsWindowController>(true);

            _unlimitedUpgradeMaterialsToggle.isOn = SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials;
            _debugToggle.isOn = SettingsCarrier.Instance.StatDebuggingMode;
            _addUpgradeMaterialsButton.interactable = !SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials;
            
            _unlimitedUpgradeMaterialsToggle.onValueChanged.AddListener(value =>
            {
                SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials = value;
                _addUpgradeMaterialsButton.interactable = !value;
                _controller.InvokeOnUpgradeMaterialAmountChanged();
            });

            _debugToggle.onValueChanged.AddListener(value =>
            {
                SettingsCarrier.Instance.StatDebuggingMode = value;
                //_controller.InvokeOnUpgradeMaterialAmountChanged();
            });

            _addUpgradeMaterialsButton.onClick.AddListener(AddUpgradeMaterials);
        }


        private void OnDestroy()
        {
            _unlimitedUpgradeMaterialsToggle.onValueChanged.RemoveAllListeners();
            _debugToggle.onValueChanged.RemoveAllListeners();
            _addUpgradeMaterialsButton.onClick.RemoveAllListeners();
        }


        /// <summary>
        /// Open DebugPopUp
        /// </summary>
        public void OpenPopUp()
        {
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
