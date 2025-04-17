using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Altzone.Scripts.BattleUiShared;
using OrientationType = Altzone.Scripts.BattleUiShared.BattleUiMultiOrientationElement.OrientationType;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Handles the functionality for BattleUiEditor prefab in Settings.
    /// </summary>
    public class BattleUiEditor : MonoBehaviour
    {
        [Header("GameObject references")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Toggle _gridToggle;
        [SerializeField] private Transform _uiElementsHolder;

        [Header("BattleUi prefabs")]
        [SerializeField] private GameObject _editingComponent;
        [SerializeField] private GameObject _timer;
        [SerializeField] private GameObject _playerInfo;
        [SerializeField] private GameObject _diamonds;
        [SerializeField] private GameObject _giveUpButton;

        public static float GridCellWidth => Screen.width / 20;
        public static float GridCellHeight => Screen.height / 40;

        /// <summary>
        /// Open and initialize BattleUiEditor
        /// </summary>
        public void OpenEditor()
        {
            gameObject.SetActive(true);

            // Instantiating ui element prefabs

            if (_instantiatedTimer == null)
            {
                _instantiatedTimer = InstantiateBattleUiElement(UiElementType.Timer);
                SetDefaultData(UiElementType.Timer);
            }

            if (_instantiatedDiamonds == null)
            {
                _instantiatedDiamonds = InstantiateBattleUiElement(UiElementType.Diamonds);
                SetDefaultData(UiElementType.Diamonds);
            }

            if (_instantiatedGiveUpButton == null)
            {
                _instantiatedGiveUpButton = InstantiateBattleUiElement(UiElementType.GiveUpButton);
                SetDefaultData(UiElementType.GiveUpButton);
            }

            if (_instantiatedPlayerInfo == null)
            {
                _instantiatedPlayerInfo = InstantiateBattleUiElement(UiElementType.PlayerInfo);
                TextMeshProUGUI playerName = _instantiatedPlayerInfo.GetComponentInChildren<TextMeshProUGUI>();
                if (playerName != null) playerName.text = "Minä";
                SetDefaultData(UiElementType.PlayerInfo);
            }

            if (_instantiatedTeammateInfo == null)
            {
                _instantiatedTeammateInfo = InstantiateBattleUiElement(UiElementType.TeammateInfo);
                TextMeshProUGUI teammateName = _instantiatedTeammateInfo.GetComponentInChildren<TextMeshProUGUI>();
                if (teammateName != null) teammateName.text = "Tiimikaveri";
                SetDefaultData(UiElementType.TeammateInfo);
            }
        }

        /// <summary>
        /// Close BattleUiEditor
        /// </summary>
        public void CloseEditor()
        {
            gameObject.SetActive(false);
        }

        enum UiElementType
        {
            Timer,
            PlayerInfo,
            TeammateInfo,
            Diamonds,
            GiveUpButton,
        }

        private GameObject _instantiatedTimer;
        private GameObject _instantiatedPlayerInfo;
        private GameObject _instantiatedTeammateInfo;
        private GameObject _instantiatedDiamonds;
        private GameObject _instantiatedGiveUpButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(CloseEditor);
            _resetButton.onClick.AddListener(()=>
            {
                SetDefaultData(UiElementType.Timer);
                SetDefaultData(UiElementType.Diamonds);
                SetDefaultData(UiElementType.GiveUpButton);
                SetDefaultData(UiElementType.PlayerInfo);
                SetDefaultData(UiElementType.TeammateInfo);
            });
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            _resetButton.onClick.RemoveAllListeners();
            _gridToggle.onValueChanged.RemoveAllListeners();
        }

        private GameObject InstantiateBattleUiElement(UiElementType uiElementType)
        {
            GameObject uiElementPrefab = null;

            switch (uiElementType)
            {
                case UiElementType.Timer:
                    uiElementPrefab = _timer;
                    break;
                case UiElementType.GiveUpButton:
                    uiElementPrefab = _giveUpButton;
                    break;
                case UiElementType.Diamonds:
                    uiElementPrefab = _diamonds;
                    break;
                case UiElementType.PlayerInfo:
                case UiElementType.TeammateInfo:
                    uiElementPrefab = _playerInfo;
                    break;
            }

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
                editingComponent.SetInfo(movableElement, _uiElementsHolder);
            }
            else if (multiOrientationElement != null)
            {
                editingComponent.SetInfo(multiOrientationElement, _uiElementsHolder);
            }
            else
            {
                return null;
            }

            // Setting listener for grid toggle
            _gridToggle.onValueChanged.AddListener(editingComponent.ToggleGrid);
            editingComponent.ToggleGrid(_gridToggle.isOn);

            return uiElementGameObject;
        }

        private BattleUiMovableElementData GetDefaultData(UiElementType uiElementType)
        {
            // Initializing variables for creating data object
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.zero;

            OrientationType orientation = OrientationType.None;

            bool isFlippedHorizontally = false;
            bool isFlippedVertically = false;

            // Rect variable so that we can do aspect ratio calculations later
            Rect uiElementRect = new();

            // Setting hardcoded default anchors (maybe there's a better way for this?)
            switch (uiElementType)
            {
                case UiElementType.Timer:
                    if (_instantiatedTimer == null) return null;

                    anchorMin.x = 0.4f;
                    anchorMax.x = 0.6f;

                    anchorMin.y = 0.45f;
                    anchorMax.y = 0.55f;

                    uiElementRect = _instantiatedTimer.GetComponent<RectTransform>().rect;
                    break;

                case UiElementType.Diamonds:
                    if (_instantiatedDiamonds == null) return null;

                    anchorMin.x = 0.75f;
                    anchorMax.x = 0.95f;

                    anchorMin.y = 0.01f;
                    anchorMax.y = 0.08f;

                    uiElementRect = _instantiatedDiamonds.GetComponent<RectTransform>().rect;
                    break;

                case UiElementType.GiveUpButton:
                    if (_instantiatedGiveUpButton == null) return null;

                    anchorMin.x = 0.05f;
                    anchorMax.x = 0.3f;

                    anchorMin.y = 0.01f;
                    anchorMax.y = 0.08f;

                    uiElementRect = _instantiatedGiveUpButton.GetComponent<RectTransform>().rect;
                    break;

                case UiElementType.PlayerInfo:
                    if (_instantiatedPlayerInfo == null) return null;

                    anchorMin.x = 0.05f;
                    anchorMax.x = 0.375f;

                    anchorMin.y = 0.45f;
                    anchorMax.y = 0.55f;

                    orientation = OrientationType.Horizontal;

                    uiElementRect = _instantiatedPlayerInfo.GetComponent<RectTransform>().rect;
                    break;

                case UiElementType.TeammateInfo:
                    if (_instantiatedTeammateInfo == null) return null;

                    anchorMin.x = 0.625f;
                    anchorMax.x = 0.95f;

                    anchorMin.y = 0.45f;
                    anchorMax.y = 0.55f;

                    orientation = OrientationType.Horizontal;

                    uiElementRect = _instantiatedTeammateInfo.GetComponent<RectTransform>().rect;
                    break;
            }

            // Fitting height to aspect ratio
            float aspectRatio = uiElementRect.width / uiElementRect.height;

            float uiElementWidth = Screen.width * (anchorMax.x - anchorMin.x);
            float uiElementHeight = uiElementWidth / aspectRatio;

            float yPos = (anchorMax.y + anchorMin.y) / 2 * Screen.height;
            anchorMin.y = (yPos - uiElementHeight / 2.0f) / Screen.height;
            anchorMax.y = (yPos + uiElementHeight / 2.0f) / Screen.height;

            return new(anchorMin, anchorMax, orientation, isFlippedHorizontally, isFlippedVertically);
        }

        private void SetDefaultData(UiElementType uiElementType)
        {
            // Getting the default data for this ui element type
            BattleUiMovableElementData data = GetDefaultData(uiElementType);
            if (data == null) return;

            // Getting the correct movable or multi orientation element and editing component
            BattleUiMovableElement movableElement = null;
            BattleUiMultiOrientationElement multiOrientationElement = null;
            BattleUiEditingComponent editingComponent = null;

            switch (uiElementType)
            {
                case UiElementType.Timer:
                    if (_instantiatedTimer == null) return;
                    movableElement = _instantiatedTimer.GetComponent<BattleUiMovableElement>();
                    editingComponent = _instantiatedTimer.GetComponentInChildren<BattleUiEditingComponent>();
                    break;
                case UiElementType.GiveUpButton:
                    if (_instantiatedGiveUpButton == null) return;
                    movableElement = _instantiatedGiveUpButton.GetComponent<BattleUiMovableElement>();
                    editingComponent = _instantiatedGiveUpButton.GetComponentInChildren<BattleUiEditingComponent>();
                    break;
                case UiElementType.Diamonds:
                    if (_instantiatedDiamonds == null) return;
                    movableElement = _instantiatedDiamonds.GetComponent<BattleUiMovableElement>();
                    editingComponent = _instantiatedDiamonds.GetComponentInChildren<BattleUiEditingComponent>();
                    break;
                case UiElementType.PlayerInfo:
                    if (_instantiatedPlayerInfo == null) return;
                    multiOrientationElement = _instantiatedPlayerInfo.GetComponent<BattleUiMultiOrientationElement>();
                    editingComponent = _instantiatedPlayerInfo.GetComponentInChildren<BattleUiEditingComponent>();
                    break;
                case UiElementType.TeammateInfo:
                    if (_instantiatedTeammateInfo == null) return;
                    multiOrientationElement = _instantiatedTeammateInfo.GetComponent<BattleUiMultiOrientationElement>();
                    editingComponent = _instantiatedTeammateInfo.GetComponentInChildren<BattleUiEditingComponent>();
                    break;
            }

            // Setting data to movable or multi orientation element
            if (movableElement != null) movableElement.SetData(data);
            else if (multiOrientationElement != null) multiOrientationElement.SetData(data);

            // Updating editing component data
            editingComponent.UpdateData();
        }
    }
}
