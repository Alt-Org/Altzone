using UnityEngine;
using UnityEngine.UI;

using Altzone.Scripts.BattleUiShared;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Handles the functionality for BattleUiEditor prefab in Settings.
    /// </summary>
    public class BattleUiEditor : MonoBehaviour
    {
        [Header("GameObject references")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _uiElementsHolder;

        [Header("BattleUi prefabs")]
        [SerializeField] private GameObject _editingComponent;
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

            _instantiatedTimer = InstantiateBattleUiElement(_timer);
            _instantiatedPlayerInfo = InstantiateBattleUiElement(_playerInfo);
            _instantiatedTeammateInfo = InstantiateBattleUiElement(_playerInfo);
            _instantiatedDiamonds = InstantiateBattleUiElement(_diamonds);
            _instantiatedGiveUpButton = InstantiateBattleUiElement(_giveUpButton);
        }

        /// <summary>
        /// Close BattleUiEditor
        /// </summary>
        public void CloseEditor()
        {
            gameObject.SetActive(false);
        }

        private GameObject _instantiatedTimer;
        private GameObject _instantiatedPlayerInfo;
        private GameObject _instantiatedTeammateInfo;
        private GameObject _instantiatedDiamonds;
        private GameObject _instantiatedGiveUpButton;

        private void OnEnable()
        {
            OpenEditor(); // for testing only
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(CloseEditor);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
        }

        private GameObject InstantiateBattleUiElement(GameObject uiElementPrefab)
        {
            if (uiElementPrefab == null) return null;
            if (_editingComponent == null) return null;

            // Instantiating gameobjects for ui element and editing component
            GameObject uiElementGameObject = Instantiate(uiElementPrefab, _uiElementsHolder);
            GameObject editingComponentGameObject = Instantiate(_editingComponent, uiElementGameObject.transform);

            // Getting editing component script from the editing component game object
            BattleUiEditingComponent editingComponent = editingComponentGameObject.GetComponent<BattleUiEditingComponent>();
            if (editingComponent == null) return null;

            // Getting movable element and multi orientation script from the ui element game object
            BattleUiMovableElement movableElement = uiElementGameObject.GetComponent<BattleUiMovableElement>();
            BattleUiMultiOrientationElement multiOrientationElement = uiElementGameObject.GetComponent<BattleUiMultiOrientationElement>();

            // Setting info to the editing component
            if (movableElement != null && multiOrientationElement == null)
            {
                editingComponent.SetInfo(movableElement);
            }
            else if (multiOrientationElement != null)
            {
                editingComponent.SetInfo(multiOrientationElement);
            }
            else
            {
                return null;
            }

            return uiElementGameObject;
        }
    }
}
