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
        [SerializeField] private Button _addCharacterButton;
        [SerializeField] private Toggle _unlimitedDiamondsToggle;
        [SerializeField] private Toggle _unlimitedErasersToggle;
        [SerializeField] private Button _addDiamondsButton;
        [SerializeField] private Button _addErasersButton;

        private StatsWindowController _controller;

        private const int DiamondsToAdd = 10000;
        private const int ErasersToAdd = 100;


        private void OnDestroy()
        {
            _addCharacterButton.onClick.RemoveAllListeners();
            _unlimitedDiamondsToggle.onValueChanged.RemoveAllListeners();
            _unlimitedDiamondsToggle.onValueChanged.RemoveAllListeners();
            _addDiamondsButton.onClick.RemoveAllListeners();
            _addErasersButton.onClick.RemoveAllListeners();
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

                _unlimitedDiamondsToggle.isOn = _controller.UnlimitedDiamonds;
                _unlimitedErasersToggle.isOn = _controller.UnlimitedErasers;
                _addDiamondsButton.interactable = !_controller.UnlimitedDiamonds;
                _addErasersButton.interactable = !_controller.UnlimitedErasers;

                _unlimitedDiamondsToggle.onValueChanged.AddListener(value =>
                {
                    _controller.UnlimitedDiamonds = value;
                    _addDiamondsButton.interactable = !value;
                });

                _unlimitedErasersToggle.onValueChanged.AddListener(value =>
                {
                    _controller.UnlimitedErasers = value;
                    _addErasersButton.interactable = !value;
                });

                _addDiamondsButton.onClick.AddListener(AddDiamonds);
                _addErasersButton.onClick.AddListener(AddErasers);
            }

            // Adding listener to add character button
            if (_controller.IsCurrentCharacterLocked())
            {
                _addCharacterButton.interactable = true;
                _addCharacterButton.onClick.RemoveAllListeners();
                _addCharacterButton.onClick.AddListener(AddCharacter);
            }
            else
            {
                _addCharacterButton.interactable = false;
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


        private void AddCharacter()
        {
            _addCharacterButton.interactable = false;

            bool success = false;
            StartCoroutine(ServerManager.Instance.AddCustomCharactersToServer(_controller.CurrentCharacterID, result =>
            {
                if (result != null)
                {
                    success = true;
                }

                if (success)
                {
                    StartCoroutine(ServerManager.Instance.UpdateCustomCharacters(result =>
                    {
                        if (result)
                        {
                            SignalBus.OnReloadCharacterGalleryRequestedSignal();
                            _controller.ReloadStatWindow();
                            ClosePopUp();
                        }
                    }
                    ));
                }
                else
                {
                    PopupSignalBus.OnChangePopupInfoSignal("T�t� hahmoa ei ole viel� lis�tty pelipalvelimelle.");
                }
            }));
        }


        private void AddDiamonds()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                playerData.DiamondSpeed += DiamondsToAdd;
                _controller.InvokeOnDiamondAmountChanged();
            }));
        }


        private void AddErasers()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                playerData.Eraser += ErasersToAdd;
                _controller.InvokeOnEraserAmountChanged();
            }));
        }
    }
}
