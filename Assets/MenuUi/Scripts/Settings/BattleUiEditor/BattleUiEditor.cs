using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Handles the functionality for BattleUiEditor prefab in Settings.
    /// </summary>
    public class BattleUiEditor : MonoBehaviour
    {
        [Header("GameObject references")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _uiElementsHolder;

        [Header("BattleUi prefabs")]
        [SerializeField] private GameObject _timer;
        [SerializeField] private GameObject _playerInfo;
        [SerializeField] private GameObject _diamonds;
        [SerializeField] private GameObject _giveUpButton;

        /// <summary>
        /// Open and initialize BattleUiEditor
        /// </summary>
        public void OpenEditor()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Close BattleUiEditor
        /// </summary>
        public void CloseEditor()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(CloseEditor);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
        }
    }
}
