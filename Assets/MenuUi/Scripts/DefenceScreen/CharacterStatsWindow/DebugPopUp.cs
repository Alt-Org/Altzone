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
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private GameObject _contents;
        [SerializeField] private Image _touchBlocker;
        [SerializeField] private Button _addCharacterButton;
        [SerializeField] private Toggle _unlimitedDiamondsToggle;
        [SerializeField] private Toggle _unlimitedErasersToggle;
        [SerializeField] private Button _addDiamondsButton;
        [SerializeField] private Button _addErasersButton;

        private void OnEnable()
        {
            ClosePopUp();
        }


        private void OnDestroy()
        {
            _addCharacterButton.onClick.RemoveAllListeners();
        }


        /// <summary>
        /// Open DebugPopUp
        /// </summary>
        public void OpenPopUp()
        {
            if (_controller.IsCurrentCharacterLocked())
            {
                _addCharacterButton.gameObject.SetActive(true);
                _addCharacterButton.onClick.RemoveAllListeners();
                _addCharacterButton.onClick.AddListener(AddCharacter);
            }
            else
            {
                _addCharacterButton.gameObject.SetActive(false);
            }

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
            _addCharacterButton.enabled = false;

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
                        }
                    }
                    ));
                }
                else
                {
                    PopupSignalBus.OnChangePopupInfoSignal("Tätä hahmoa ei ole vielä lisätty pelipalvelimelle.");
                }

                _addCharacterButton.enabled = true;
            }));
        }
    }
}
