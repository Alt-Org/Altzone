using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Altzone.Scripts.BattleUiShared;
using OrientationType = Altzone.Scripts.BattleUiShared.BattleUiMultiOrientationElement.OrientationType;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;

using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Handles the functionality for BattleUiEditor prefab in Settings.
    /// </summary>
    public class BattleUiEditor : MonoBehaviour
    {
        [Header("GameObject references")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Toggle _gridToggle;
        [SerializeField] private Transform _uiElementsHolder;

        [Header("BattleUi prefabs")]
        [SerializeField] private GameObject _editingComponent;
        [SerializeField] private GameObject _timer;
        [SerializeField] private GameObject _playerInfo;
        [SerializeField] private GameObject _diamonds;
        [SerializeField] private GameObject _giveUpButton;

        [Header("Save changes popup")]
        [SerializeField] private GameObject _saveChangesPopup;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _noButton;

        public static float GridCellWidth => Screen.width / 20;
        public static float GridCellHeight => Screen.height / 40;

        /// <summary>
        /// Open and initialize BattleUiEditor
        /// </summary>
        public void OpenEditor()
        {
            gameObject.SetActive(true);

            // Instantiating ui element prefabs

            if (_instantiatedTimer == null) _instantiatedTimer = InstantiateBattleUiElement(BattleUiElementType.Timer);
            SetData(BattleUiElementType.Timer);

            if (_instantiatedDiamonds == null) _instantiatedDiamonds = InstantiateBattleUiElement(BattleUiElementType.Diamonds);
            SetData(BattleUiElementType.Diamonds);

            if (_instantiatedGiveUpButton == null) _instantiatedGiveUpButton = InstantiateBattleUiElement(BattleUiElementType.GiveUpButton);
            SetData(BattleUiElementType.GiveUpButton);

            if (_instantiatedPlayerInfo == null)
            {
                _instantiatedPlayerInfo = InstantiateBattleUiElement(BattleUiElementType.PlayerInfo);
                TextMeshProUGUI playerName = _instantiatedPlayerInfo.GetComponentInChildren<TextMeshProUGUI>();
                if (playerName != null) playerName.text = "Minä";
            }
            SetData(BattleUiElementType.PlayerInfo);

            if (_instantiatedTeammateInfo == null)
            {
                _instantiatedTeammateInfo = InstantiateBattleUiElement(BattleUiElementType.TeammateInfo);
                TextMeshProUGUI teammateName = _instantiatedTeammateInfo.GetComponentInChildren<TextMeshProUGUI>();
                if (teammateName != null) teammateName.text = "Tiimikaveri";
            }
            SetData(BattleUiElementType.TeammateInfo);
        }

        /// <summary>
        /// Close BattleUiEditor, before closing show save changes popup.
        /// </summary>
        public void CloseEditor()
        {
            if (_unsavedChanges)
            {
                StartCoroutine(ShowSaveChangesPopup(saveChanges =>
                {
                    if (saveChanges == null) return;
                    if (saveChanges.Value == true) SaveChanges();
                    gameObject.SetActive(false);
                }));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Close save changes popup
        /// </summary>
        public void CloseSaveChangesPopup()
        {
            _saveChangesPopup.SetActive(false);
        }

        private GameObject _instantiatedTimer;
        private GameObject _instantiatedPlayerInfo;
        private GameObject _instantiatedTeammateInfo;
        private GameObject _instantiatedDiamonds;
        private GameObject _instantiatedGiveUpButton;

        private bool _unsavedChanges = false;

        private BattleUiEditingComponent _currentlySelectedEditingComponent;

        private void Awake()
        {
            _closeButton.onClick.AddListener(CloseEditor);
            _resetButton.onClick.AddListener(() =>
            {
                SetDefaultData(BattleUiElementType.Timer);
                SetDefaultData(BattleUiElementType.Diamonds);
                SetDefaultData(BattleUiElementType.GiveUpButton);
                SetDefaultData(BattleUiElementType.PlayerInfo);
                SetDefaultData(BattleUiElementType.TeammateInfo);
            });
            _saveButton.onClick.AddListener(SaveChanges);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            _resetButton.onClick.RemoveAllListeners();
            _gridToggle.onValueChanged.RemoveAllListeners();
            _saveButton.onClick.RemoveAllListeners();
            _okButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();

            foreach (var editingComponent in GetComponentsInChildren<BattleUiEditingComponent>())
            {
                editingComponent.OnUiElementEdited -= OnUiElementEdited;
                editingComponent.OnUiElementSelected -= OnUiElementSelected;
            }
        }

        private IEnumerator ShowSaveChangesPopup(Action<bool?> callback)
        {
            _saveChangesPopup.SetActive(true);

            _okButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();

            bool? saveChanges = null;

            _okButton.onClick.AddListener(() => saveChanges = true);
            _noButton.onClick.AddListener(() => saveChanges = false);

            yield return new WaitUntil(() => saveChanges.HasValue || !_saveChangesPopup.activeSelf);

            if (_saveChangesPopup.activeSelf) CloseSaveChangesPopup();

            callback(saveChanges);
        }

        private void SaveChanges()
        {
            BattleUiMovableElementData timerData = GetMovableElement(BattleUiElementType.Timer).GetData();
            SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.Timer, timerData);

            BattleUiMovableElementData diamondsData = GetMovableElement(BattleUiElementType.Diamonds).GetData();
            SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.Diamonds, diamondsData);

            BattleUiMovableElementData giveUpButtonData = GetMovableElement(BattleUiElementType.GiveUpButton).GetData();
            SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.GiveUpButton, giveUpButtonData);

            BattleUiMovableElementData playerInfoData = GetMultiOrientationElement(BattleUiElementType.PlayerInfo).GetData();
            SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.PlayerInfo, playerInfoData);

            BattleUiMovableElementData teammateInfoData = GetMultiOrientationElement(BattleUiElementType.TeammateInfo).GetData();
            SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.TeammateInfo, teammateInfoData);

            _unsavedChanges = false;
            PopupSignalBus.OnChangePopupInfoSignal("Muutokset on tallennettu.");
        }

        private GameObject InstantiateBattleUiElement(BattleUiElementType uiElementType)
        {
            GameObject uiElementPrefab = null;

            switch (uiElementType)
            {
                case BattleUiElementType.Timer:
                    uiElementPrefab = _timer;
                    break;
                case BattleUiElementType.GiveUpButton:
                    uiElementPrefab = _giveUpButton;
                    break;
                case BattleUiElementType.Diamonds:
                    uiElementPrefab = _diamonds;
                    break;
                case BattleUiElementType.PlayerInfo:
                case BattleUiElementType.TeammateInfo:
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

            // Setting listener for editing component events
            editingComponent.OnUiElementEdited += OnUiElementEdited;
            editingComponent.OnUiElementSelected += OnUiElementSelected;

            return uiElementGameObject;
        }

        private void SetData(BattleUiElementType uiElementType)
        {
            // Getting the saved data for this ui element type
            BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(uiElementType);

            // Setting default data if saved data is null
            if (data == null)
            {
                SetDefaultData(uiElementType);
                return;
            }

            // Getting the movable or multi orientation element and editing component
            BattleUiMovableElement movableElement = GetMovableElement(uiElementType);
            BattleUiMultiOrientationElement multiOrientationElement = GetMultiOrientationElement(uiElementType);
            BattleUiEditingComponent editingComponent = GetEditingComponent(uiElementType);

            // Setting data to movable or multi orientation element
            if (movableElement != null) movableElement.SetData(data);
            else if (multiOrientationElement != null) multiOrientationElement.SetData(data);

            // Updating editing component data
            editingComponent.UpdateData();
        }

        private void SetDefaultData(BattleUiElementType uiElementType)
        {
            // Getting the default data for this ui element type
            BattleUiMovableElementData data = GetDefaultData(uiElementType);
            if (data == null) return;

            // Getting the movable or multi orientation element and editing component
            BattleUiMovableElement movableElement = GetMovableElement(uiElementType);
            BattleUiMultiOrientationElement multiOrientationElement = GetMultiOrientationElement(uiElementType);
            BattleUiEditingComponent editingComponent = GetEditingComponent(uiElementType);

            // Setting data to movable or multi orientation element
            if (movableElement != null) movableElement.SetData(data);
            else if (multiOrientationElement != null) multiOrientationElement.SetData(data);

            // Updating editing component data
            editingComponent.UpdateData();
        }

        private BattleUiMovableElementData GetDefaultData(BattleUiElementType uiElementType)
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
                case BattleUiElementType.Timer:
                    if (_instantiatedTimer == null) return null;

                    anchorMin.x = 0.4f;
                    anchorMax.x = 0.6f;

                    anchorMin.y = 0.45f;
                    anchorMax.y = 0.55f;

                    uiElementRect = _instantiatedTimer.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.Diamonds:
                    if (_instantiatedDiamonds == null) return null;

                    anchorMin.x = 0.75f;
                    anchorMax.x = 0.95f;

                    anchorMin.y = 0.01f;
                    anchorMax.y = 0.08f;

                    uiElementRect = _instantiatedDiamonds.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.GiveUpButton:
                    if (_instantiatedGiveUpButton == null) return null;

                    anchorMin.x = 0.05f;
                    anchorMax.x = 0.3f;

                    anchorMin.y = 0.01f;
                    anchorMax.y = 0.08f;

                    uiElementRect = _instantiatedGiveUpButton.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.PlayerInfo:
                    if (_instantiatedPlayerInfo == null) return null;

                    anchorMin.x = 0.05f;
                    anchorMax.x = 0.375f;

                    anchorMin.y = 0.45f;
                    anchorMax.y = 0.55f;

                    orientation = OrientationType.Horizontal;

                    uiElementRect = _instantiatedPlayerInfo.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.TeammateInfo:
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

        private BattleUiMovableElement GetMovableElement(BattleUiElementType uiElementType)
        {
            switch (uiElementType)
            {
                case BattleUiElementType.Timer:
                    if (_instantiatedTimer == null) return null;
                    return _instantiatedTimer.GetComponent<BattleUiMovableElement>();

                case BattleUiElementType.GiveUpButton:
                    if (_instantiatedGiveUpButton == null) return null;
                    return _instantiatedGiveUpButton.GetComponent<BattleUiMovableElement>();

                case BattleUiElementType.Diamonds:
                    if (_instantiatedDiamonds == null) return null;
                    return _instantiatedDiamonds.GetComponent<BattleUiMovableElement>();

                case BattleUiElementType.PlayerInfo:
                case BattleUiElementType.TeammateInfo:
                default:
                    return null;
            }
        }

        private BattleUiMultiOrientationElement GetMultiOrientationElement(BattleUiElementType uiElementType)
        {
            switch (uiElementType)
            {
                case BattleUiElementType.PlayerInfo:
                    if (_instantiatedPlayerInfo == null) return null;
                    return _instantiatedPlayerInfo.GetComponent<BattleUiMultiOrientationElement>();

                case BattleUiElementType.TeammateInfo:
                    if (_instantiatedTeammateInfo == null) return null;
                    return _instantiatedTeammateInfo.GetComponent<BattleUiMultiOrientationElement>();

                case BattleUiElementType.Timer:
                case BattleUiElementType.GiveUpButton:
                case BattleUiElementType.Diamonds:
                default:
                    return null;
            }
        }

        private BattleUiEditingComponent GetEditingComponent(BattleUiElementType uiElementType)
        {
            switch (uiElementType)
            {
                case BattleUiElementType.Timer:
                    if (_instantiatedTimer == null) return null;
                    return _instantiatedTimer.GetComponentInChildren<BattleUiEditingComponent>();

                case BattleUiElementType.GiveUpButton:
                    if (_instantiatedGiveUpButton == null) return null;
                    return _instantiatedGiveUpButton.GetComponentInChildren<BattleUiEditingComponent>();

                case BattleUiElementType.Diamonds:
                    if (_instantiatedDiamonds == null) return null;
                    return _instantiatedDiamonds.GetComponentInChildren<BattleUiEditingComponent>();

                case BattleUiElementType.PlayerInfo:
                    if (_instantiatedPlayerInfo == null) return null;
                    return _instantiatedPlayerInfo.GetComponentInChildren<BattleUiEditingComponent>();

                case BattleUiElementType.TeammateInfo:
                    if (_instantiatedTeammateInfo == null) return null;
                    return _instantiatedTeammateInfo.GetComponentInChildren<BattleUiEditingComponent>();

                default:
                    return null;
            }
        }

        private void OnUiElementEdited()
        {
            _unsavedChanges = true;
        }

        private void OnUiElementSelected(BattleUiEditingComponent newSelectedEditingComponent)
        {
            if (_currentlySelectedEditingComponent != null && _currentlySelectedEditingComponent != newSelectedEditingComponent)
            {
                _currentlySelectedEditingComponent.ShowControls(false);
            }

            _currentlySelectedEditingComponent = newSelectedEditingComponent;
        }
    }
}
