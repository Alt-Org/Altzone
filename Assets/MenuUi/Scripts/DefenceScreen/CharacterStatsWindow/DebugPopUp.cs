using MenuUI.Scripts;
using UnityEngine;
using UnityEngine.UI;

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
            bool success = false;
            StartCoroutine(ServerManager.Instance.AddCustomCharactersToServer(_controller.CurrentCharacterID, result =>
            {
                if (result != null)
                {
                    success = true;
                }
            }));

            if (success)
            {
                StartCoroutine(ServerManager.Instance.UpdateCustomCharacters(null));
            }
            else
            {
                SignalBus.OnChangePopupInfoSignal("Tätä hahmoa ei ole vielä lisätty pelipalvelimelle.");
            }
        }
    }
}
