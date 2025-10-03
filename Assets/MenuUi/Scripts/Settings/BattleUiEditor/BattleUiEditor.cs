using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Altzone.Scripts.BattleUiShared;
using OrientationType = Altzone.Scripts.BattleUiShared.BattleUiMultiOrientationElement.OrientationType;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;

using MenuUi.Scripts.UIScaling;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Handles the functionality for BattleUiEditor prefab in Settings.
    /// </summary>
    public class BattleUiEditor : MonoBehaviour
    {
        [Header("Editor GameObject references")]
        [SerializeField] private RectTransform _editorRectTransform;
        [SerializeField] private RectTransform _topButtonsRectTransform;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _previewButton;
        [SerializeField] private Button _previewModeTouchDetector;
        [Space]
        [SerializeField] private GameObject _uiTransparencyHolder;
        [SerializeField] private Slider _uiTransparencySlider;
        [SerializeField] private TMP_InputField _uiTransparencyInputField;
        [Space]
        [SerializeField] private RectTransform _uiElementsHolder;
        [SerializeField] private GridController _grid;

        [Header("Options popup")]
        [SerializeField] private Button _optionsButton;
        [SerializeField] private OptionsPopup _optionsPopup;

        [Header("Save/reset popup")]
        [SerializeField] private SaveReset _saveReset;

        [Header("BattleUi prefabs")]
        [SerializeField] private GameObject _editingComponent;
        [SerializeField] private BattleUiPrefabs _prefabs;

        public enum CornerType // Helper enum to access button world corners and scale handles array in editing component script more readably.
        {
            BottomLeft = 0,
            TopLeft = 1,
            TopRight = 2,
            BottomRight = 3,
        }

        public static Rect EditorRect;
        public static RectTransform EditorRectTransform;

        /// <summary>
        /// Calculate anchors based on Ui element size and position.
        /// </summary>
        /// <param name="size">Size of the Ui element.</param>
        /// <param name="pos">Ui element position.</param>
        /// <param name="offset">Offset for calculating the anchors.</param>
        /// <param name="useUiElementsHolder">If calculation should use Ui elements holder rect instead of editor rect.</param>
        /// <returns>Two Vector2, anchorMin and anchorMax.</returns>
        public static (Vector2 anchorMin, Vector2 anchorMax) CalculateAnchors(Vector2 size, Vector2 pos, float offset = 0f, bool useUiElementsHolder = false)
        {
            Vector2 holderSize = useUiElementsHolder ? s_uiElementsHolder.rect.size : new(EditorRect.width, EditorRect.height);

            // Calculating anchors
            float anchorXMin = Mathf.Clamp01((pos.x - size.x * 0.5f) / holderSize.x + offset);
            float anchorXMax = Mathf.Clamp01((pos.x + size.x * 0.5f) / holderSize.x + offset);

            float anchorYMin = Mathf.Clamp01((pos.y - size.y * 0.5f) / holderSize.y + offset);
            float anchorYMax = Mathf.Clamp01((pos.y + size.y * 0.5f) / holderSize.y + offset);

            return (new Vector2(anchorXMin, anchorYMin), new Vector2(anchorXMax, anchorYMax));
        }

        /// <summary>
        /// Open and initialize BattleUiEditor
        /// </summary>
        public void OpenEditor()
        {
            gameObject.SetActive(true);
            _optionsPopup.OpenOptionsPopup();

            // Instantiating Ui element prefabs
            if (_instantiatedTimer == null) _instantiatedTimer = InstantiateBattleUiElement(BattleUiElementType.Timer).GetComponent<BattleUiMovableElement>();
            if (_instantiatedDiamonds == null) _instantiatedDiamonds = InstantiateBattleUiElement(BattleUiElementType.Diamonds).GetComponent<BattleUiMovableElement>();
            if (_instantiatedGiveUpButton == null) _instantiatedGiveUpButton = InstantiateBattleUiElement(BattleUiElementType.GiveUpButton).GetComponent<BattleUiMovableElement>();

            if (_instantiatedPlayerInfo == null) _instantiatedPlayerInfo = InstantiateBattleUiElement(BattleUiElementType.PlayerInfo).GetComponent<BattleUiMultiOrientationElement>();
            if (_instantiatedTeammateInfo == null) _instantiatedTeammateInfo = InstantiateBattleUiElement(BattleUiElementType.TeammateInfo).GetComponent<BattleUiMultiOrientationElement>();

            // Setting data to Ui elements
            SetDataToUiElement(_instantiatedTimer);
            SetDataToUiElement(_instantiatedDiamonds);
            SetDataToUiElement(_instantiatedGiveUpButton);

            SetDataToUiElement(_instantiatedPlayerInfo);
            SetDataToUiElement(_instantiatedTeammateInfo);

            if (_instantiatedMoveJoystick != null) SetDataToUiElement(_instantiatedMoveJoystick);
            if (_instantiatedRotateJoystick != null) SetDataToUiElement(_instantiatedRotateJoystick);
        }

        /// <summary>
        /// Close BattleUiEditor, before closing show save changes popup.
        /// </summary>
        private void CloseEditor()
        {
            ClosePopups();
            if (_unsavedChanges)
            {
                OnUiElementSelected(null);
                StartCoroutine(_saveReset.ShowSaveResetPopup(SaveChangesText, saveChanges =>
                {
                    if (saveChanges == null) return;
                    if (saveChanges.Value == true) SaveChanges();
                    else _unsavedChanges = false;
                    gameObject.SetActive(false);
                }));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Function which closes any editor popups.
        /// </summary>
        public void ClosePopups()
        {
            _saveReset.CloseSaveResetPopup();
            _optionsPopup.CloseOptionsPopup();
            OnUiElementSelected(null);
        }

        private const string PlayerText = "Minä";
        private const string TeammateText = "Tiimikaveri";

        private const string SaveChangesText = "Tallenna muutokset?";

        private const string AlignToGridKey = "BattleUiEditorAlignToGrid";
        private const string IncrementalScalingKey = "BattleUiEditorIncScaling";

        private const float TopButtonsHeight = 0.05f;

        private static RectTransform s_uiElementsHolder;

        private BattleUiMovableElement _instantiatedTimer;
        private BattleUiMultiOrientationElement _instantiatedPlayerInfo;
        private BattleUiMultiOrientationElement _instantiatedTeammateInfo;
        private BattleUiMovableElement _instantiatedDiamonds;
        private BattleUiMovableElement _instantiatedGiveUpButton;
        public BattleUiMovableElement _instantiatedMoveJoystick;
        public BattleUiMovableElement _instantiatedRotateJoystick;

        private readonly List<BattleUiEditingComponent> _editingComponents = new();

        private BattleUiEditingComponent _timerEditingComponent;
        private BattleUiEditingComponent _playerInfoEditingComponent;
        private BattleUiEditingComponent _teammateInfoEditingComponent;
        private BattleUiEditingComponent _diamondsEditingComponent;
        private BattleUiEditingComponent _giveUpButtonEditingComponent;
        private BattleUiEditingComponent _moveJoystickEditingComponent;
        private BattleUiEditingComponent _rotateJoystickEditingComponent;

        private bool _unsavedChanges = false;

        private BattleUiEditingComponent _currentlySelectedEditingComponent;

        private void Awake()
        {
            // Assign static editor rect variables
            EditorRectTransform = _editorRectTransform;
            EditorRect = EditorRectTransform.rect;
            s_uiElementsHolder = _uiElementsHolder;

            // Scale editor to account for unsafe area
            ScaleEditor();

            // Close and save button listeners
            _closeButton.onClick.AddListener(CloseEditor);
            _saveButton.onClick.AddListener(SaveChanges);

            // Preview mode listeners
            _previewButton.onClick.AddListener(OpenPreviewMode);
            _previewModeTouchDetector.onClick.AddListener(ClosePreviewMode);

            // Ui element transparency listeners
            _uiTransparencySlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _uiTransparencyInputField);
                _currentlySelectedEditingComponent.UpdateTransparency((int)value);
            });
            _uiTransparencyInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_uiTransparencyInputField, _uiTransparencySlider);
                _currentlySelectedEditingComponent.UpdateTransparency((int)_uiTransparencySlider.value);
            });

            // Options popup listeners
            _optionsButton.onClick.AddListener(_optionsPopup.ToggleOptionsPopup);
        }

        private void OnDestroy()
        {
            // Removing close and save button listeners
            _closeButton.onClick.RemoveAllListeners();
            _saveButton.onClick.RemoveAllListeners();

            // Removing preview mode listeners
            _previewButton.onClick.RemoveAllListeners();
            _previewModeTouchDetector.onClick.RemoveAllListeners();

            // Removing editing component listeners
            foreach (BattleUiEditingComponent editingComponent in _editingComponents)
            {
                editingComponent.OnUiElementEdited -= OnUiElementEdited;
                editingComponent.OnUiElementSelected -= OnUiElementSelected;
                editingComponent.OnGridSnap -= _grid.HighlightLines;
            }

            // Removing options button listeners
            _optionsButton.onClick.RemoveAllListeners();
        }

        private void ScaleEditor()
        {
            float unsafeAreaHeight = PanelScaler.CalculateUnsafeAreaHeight();

            // Calculating max y anchor for the editor
            float anchorMaxY = 1 - TopButtonsHeight - unsafeAreaHeight;

            // Calculating editor aspect ratio and size
            float editorAspectRatio = (float)Screen.width / Screen.height;
            float editorHeight = Screen.height * anchorMaxY;
            float editorWidth = editorHeight * editorAspectRatio;

            // Calculating x anchors
            float widthAnchorValue = editorWidth / Screen.width;
            float anchorMinX = (1 - widthAnchorValue) * 0.5f;
            float anchorMaxX = widthAnchorValue + anchorMinX;

            // Setting editor anchors
            EditorRectTransform.anchorMin = new(anchorMinX, 0);
            EditorRectTransform.anchorMax = new(anchorMaxX, anchorMaxY);

            // Setting top button anchors
            _topButtonsRectTransform.anchorMin = new(0, anchorMaxY);
            _topButtonsRectTransform.anchorMax = new(1, 1 - unsafeAreaHeight);
        }

        private void OpenPreviewMode()
        {
            OnUiElementSelected(null);
            _optionsPopup.CloseOptionsPopup();
            _topButtonsRectTransform.gameObject.SetActive(false);
            _previewModeTouchDetector.gameObject.SetActive(true);
            EditorRectTransform.anchorMin = Vector2.zero;
            EditorRectTransform.anchorMax = Vector2.one;
        }

        private void ClosePreviewMode()
        {
            _topButtonsRectTransform.gameObject.SetActive(true);
            _previewModeTouchDetector.gameObject.SetActive(false);
            ScaleEditor();
        }

        private void SaveChanges()
        {
            BattleUiMovableElementData timerData = _instantiatedTimer.GetData();
            if (!IsSavedDataSimilar(BattleUiElementType.Timer)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.Timer, timerData);

            BattleUiMovableElementData diamondsData = _instantiatedDiamonds.GetData();
            if (!IsSavedDataSimilar(BattleUiElementType.Diamonds)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.Diamonds, diamondsData);

            BattleUiMovableElementData giveUpButtonData = _instantiatedGiveUpButton.GetData();
            if (!IsSavedDataSimilar(BattleUiElementType.GiveUpButton)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.GiveUpButton, giveUpButtonData);

            BattleUiMovableElementData playerInfoData = _instantiatedPlayerInfo.GetData();
            if (!IsSavedDataSimilar(BattleUiElementType.PlayerInfo)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.PlayerInfo, playerInfoData);

            BattleUiMovableElementData teammateInfoData = _instantiatedTeammateInfo.GetData();
            if (!IsSavedDataSimilar(BattleUiElementType.TeammateInfo)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.TeammateInfo, teammateInfoData);

            if (_instantiatedMoveJoystick != null) // Joysticks might not be initialized so doing a null check
            {
                BattleUiMovableElementData moveJoystickData = _instantiatedMoveJoystick.GetData();
                if (!IsSavedDataSimilar(BattleUiElementType.MoveJoystick)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.MoveJoystick, moveJoystickData);
            }

            if (_instantiatedRotateJoystick != null)
            {
                BattleUiMovableElementData rotateJoystickData = _instantiatedRotateJoystick.GetData();
                if (!IsSavedDataSimilar(BattleUiElementType.RotateJoystick)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.RotateJoystick, rotateJoystickData);
            }

            _unsavedChanges = false;
            PopupSignalBus.OnChangePopupInfoSignal("Muutokset on tallennettu.");
        }

        public void ResetChanges()
        {
            SetDefaultDataToUiElement(_instantiatedTimer);
            SetDefaultDataToUiElement(_instantiatedDiamonds);
            SetDefaultDataToUiElement(_instantiatedGiveUpButton);
            SetDefaultDataToUiElement(_instantiatedPlayerInfo);
            SetDefaultDataToUiElement(_instantiatedTeammateInfo);
            SetDefaultDataToUiElement(_instantiatedMoveJoystick);
            SetDefaultDataToUiElement(_instantiatedRotateJoystick);

            _unsavedChanges = !IsSavedDataSimilar();
        }

        private bool IsSavedDataSimilar(BattleUiElementType uiElementType = BattleUiElementType.None)
        {
            BattleUiMovableElementData savedData = SettingsCarrier.Instance.GetBattleUiMovableElementData(uiElementType);
            BattleUiMovableElementData compareData;

            switch (uiElementType)
            {
                case BattleUiElementType.Timer:
                    compareData = _instantiatedTimer.GetData();
                    break;
                case BattleUiElementType.Diamonds:
                    compareData = _instantiatedDiamonds.GetData();
                    break;
                case BattleUiElementType.GiveUpButton:
                    compareData = _instantiatedGiveUpButton.GetData();
                    break;
                case BattleUiElementType.PlayerInfo:
                    compareData = _instantiatedPlayerInfo.GetData();
                    break;
                case BattleUiElementType.TeammateInfo:
                    compareData = _instantiatedTeammateInfo.GetData();
                    break;
                case BattleUiElementType.MoveJoystick:
                    if (_instantiatedMoveJoystick == null) return true;
                    compareData = _instantiatedMoveJoystick.GetData();
                    break;
                case BattleUiElementType.RotateJoystick:
                    if (_instantiatedRotateJoystick == null) return true;
                    compareData = _instantiatedRotateJoystick.GetData();
                    break;
                default: // Checking if saved data is similar for every ui element
                    // Note: if more ui elements are added change from GiveUpButton to the last element in the enum
                    bool isSavedDataSimilar = true;
                    for (int i = 0; i <= (int)BattleUiElementType.RotateJoystick; i++)
                    {
                        isSavedDataSimilar = IsSavedDataSimilar((BattleUiElementType)i);
                        if (!isSavedDataSimilar) break;
                    }
                    return isSavedDataSimilar;
            }

            if (savedData == null || compareData == null) return false;

            // Compare if the data is similar
            return savedData.IsFlippedHorizontally == compareData.IsFlippedHorizontally
                && savedData.IsFlippedVertically == compareData.IsFlippedVertically
                && savedData.AnchorMin == compareData.AnchorMin
                && savedData.AnchorMax == compareData.AnchorMax
                && savedData.Orientation == compareData.Orientation
                && savedData.HandleSize == compareData.HandleSize
                && savedData.Transparency == compareData.Transparency;
        }

        public GameObject InstantiateBattleUiElement(BattleUiElementType uiElementType)
        {
            if (uiElementType == BattleUiElementType.None) return null;

            GameObject uiElementPrefab = null;

            switch (uiElementType)
            {
                case BattleUiElementType.Timer:
                    uiElementPrefab = _prefabs.Timer;
                    break;
                case BattleUiElementType.GiveUpButton:
                    uiElementPrefab = _prefabs.GiveUpButton;
                    break;
                case BattleUiElementType.Diamonds:
                    uiElementPrefab = _prefabs.Diamonds;
                    break;
                case BattleUiElementType.PlayerInfo:
                case BattleUiElementType.TeammateInfo:
                    uiElementPrefab = _prefabs.PlayerInfo;
                    break;
                case BattleUiElementType.MoveJoystick:
                case BattleUiElementType.RotateJoystick:
                    uiElementPrefab = _prefabs.Joystick;
                    break;
            }

            if (uiElementPrefab == null) return null;
            if (_editingComponent == null) return null;

            // Instantiating gameobjects for ui element and editing component
            GameObject uiElementGameObject = Instantiate(uiElementPrefab, _uiElementsHolder);
            GameObject editingComponentGameObject = Instantiate(_editingComponent, uiElementGameObject.transform);

            uiElementGameObject.SetActive(true);
            editingComponentGameObject.SetActive(true);

            // Getting editing component script from the editing component game object
            BattleUiEditingComponent editingComponent = editingComponentGameObject.GetComponent<BattleUiEditingComponent>();
            if (editingComponent == null) return null;

            // Setting info to the editing component
            BattleUiMultiOrientationElement multiOrientationElement = uiElementGameObject.GetComponent<BattleUiMultiOrientationElement>();
            if (multiOrientationElement != null)
            {
                editingComponent.SetInfo(multiOrientationElement);
            }
            else
            {
                BattleUiMovableJoystickElement movableJoystickElement = uiElementGameObject.GetComponent<BattleUiMovableJoystickElement>();

                if (movableJoystickElement != null)
                {
                    editingComponent.SetInfo(movableJoystickElement);
                }
                else
                {
                    BattleUiMovableElement movableElement = uiElementGameObject.GetComponent<BattleUiMovableElement>();
                    editingComponent.SetInfo(movableElement);
                }
            }

            // Setting listener for toggles
            _optionsPopup._alignToGridToggle.onValueChanged.AddListener((value) =>
            {
                editingComponent.ToggleGrid(value);
                PlayerPrefs.SetInt(AlignToGridKey, value ? 1 : 0);
            });
            editingComponent.ToggleGrid(_optionsPopup._alignToGridToggle.isOn);

            _optionsPopup._incrementalScalingToggle.onValueChanged.AddListener((value) =>
            {
                editingComponent.ToggleIncrementScaling(value);
                PlayerPrefs.SetInt(IncrementalScalingKey, value ? 1 : 0);
            });
            editingComponent.ToggleIncrementScaling(_optionsPopup._incrementalScalingToggle.isOn);

            // Setting listener for editing component events
            editingComponent.OnUiElementEdited += OnUiElementEdited;
            editingComponent.OnUiElementSelected += OnUiElementSelected;
            editingComponent.OnGridSnap += _grid.HighlightLines;

            // Saving references for the editing component
            _editingComponents.Add(editingComponent);

            switch (uiElementType)
            {
                case BattleUiElementType.Timer:
                    _timerEditingComponent = editingComponent;
                    break;
                case BattleUiElementType.GiveUpButton:
                    _giveUpButtonEditingComponent = editingComponent;
                    break;
                case BattleUiElementType.Diamonds:
                    _diamondsEditingComponent = editingComponent;
                    break;
                case BattleUiElementType.PlayerInfo:
                    _playerInfoEditingComponent = editingComponent;
                    break;
                case BattleUiElementType.TeammateInfo:
                    _teammateInfoEditingComponent = editingComponent;
                    break;
                case BattleUiElementType.MoveJoystick:
                    _moveJoystickEditingComponent = editingComponent;
                    break;
                case BattleUiElementType.RotateJoystick:
                    _rotateJoystickEditingComponent = editingComponent;
                    break;
            }

            // Initializing visuals if needed to differentiate them
            switch (uiElementType)
            {
                case BattleUiElementType.PlayerInfo:
                case BattleUiElementType.TeammateInfo:
                    TextMeshProUGUI playerNameHorizontal = multiOrientationElement.HorizontalConfiguration.GetComponentInChildren<TextMeshProUGUI>();
                    TextMeshProUGUI playerNameVertical = multiOrientationElement.VerticalConfiguration.GetComponentInChildren<TextMeshProUGUI>();

                    string nameText = uiElementType == BattleUiElementType.PlayerInfo ? PlayerText : TeammateText;

                    if (playerNameHorizontal != null) playerNameHorizontal.text = nameText;
                    if (playerNameVertical != null) playerNameVertical.text = nameText;
                    break;
            }

            return uiElementGameObject;
        }

        public void SetDataToUiElement(BattleUiMovableElement movableElement)
        {
            if (movableElement == null) return;

            // Getting ui element type
            BattleUiElementType uiElementType = GetUiElementType(movableElement);
            if (uiElementType == BattleUiElementType.None) return;

            // Getting the saved data for this ui element type
            BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(uiElementType);

            // Setting default data if saved data is null
            if (data == null)
            {
                SetDefaultDataToUiElement(movableElement);
                return;
            }

            // Setting data to movable element
            movableElement.SetData(data);

            // Updating editing component data
            BattleUiEditingComponent editingComponent = GetEditingComponent(uiElementType);
            editingComponent.UpdateData();
        }

        private void SetDataToUiElement(BattleUiMultiOrientationElement multiOrientationElement)
        {
            if (multiOrientationElement == null) return;

            // Getting ui element type
            BattleUiElementType uiElementType = GetUiElementType(multiOrientationElement);
            if (uiElementType == BattleUiElementType.None) return;

            // Getting the saved data for this ui element type
            BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(uiElementType);

            // Setting default data if saved data is null
            if (data == null)
            {
                SetDefaultDataToUiElement(multiOrientationElement);
                return;
            }

            // Setting data to multiorientation element
            multiOrientationElement.SetData(data);

            // Updating editing component data
            BattleUiEditingComponent editingComponent = GetEditingComponent(uiElementType);
            editingComponent.UpdateData();
        }

        private void SetDefaultDataToUiElement(BattleUiMovableElement movableElement)
        {
            if (movableElement == null) return;

            // Getting ui element type
            BattleUiElementType uiElementType = GetUiElementType(movableElement);
            if (uiElementType == BattleUiElementType.None) return;

            // Getting the default data for this ui element type
            BattleUiMovableElementData data = GetDefaultDataForUiElement(uiElementType);
            if (data == null) return;

            // Getting the movable or multi orientation element and editing component

            // Setting data to movable element
            movableElement.SetData(data);

            // Updating editing component data
            BattleUiEditingComponent editingComponent = GetEditingComponent(uiElementType);
            editingComponent.UpdateData();
        }

        private void SetDefaultDataToUiElement(BattleUiMultiOrientationElement multiOrientationElement)
        {
            if (multiOrientationElement == null) return;

            // Getting ui element type
            BattleUiElementType uiElementType = GetUiElementType(multiOrientationElement);
            if (uiElementType == BattleUiElementType.None) return;

            // Getting the default data for this ui element type
            BattleUiMovableElementData data = GetDefaultDataForUiElement(uiElementType);
            if (data == null) return;

            // Setting data to multi orientation element
            multiOrientationElement.SetData(data);

            // Updating editing component data
            BattleUiEditingComponent editingComponent = GetEditingComponent(uiElementType);
            editingComponent.UpdateData();
        }

        private BattleUiElementType GetUiElementType(BattleUiMovableElement movableElement)
        {
            if (movableElement == _instantiatedTimer)
            {
                return BattleUiElementType.Timer;
            }
            else if (movableElement == _instantiatedGiveUpButton)
            {
                return BattleUiElementType.GiveUpButton;
            }
            else if (movableElement == _instantiatedDiamonds)
            {
                return BattleUiElementType.Diamonds;
            }
            else if (movableElement == _instantiatedMoveJoystick)
            {
                return BattleUiElementType.MoveJoystick;
            }
            else if (movableElement == _instantiatedRotateJoystick)
            {
                return BattleUiElementType.RotateJoystick;
            }
            else
            {
                return BattleUiElementType.None;
            }
        }

        private BattleUiElementType GetUiElementType(BattleUiMultiOrientationElement multiOrientationElement)
        {
            if (multiOrientationElement == _instantiatedPlayerInfo)
            {
                return BattleUiElementType.PlayerInfo;
            }
            else if (multiOrientationElement == _instantiatedTeammateInfo)
            {
                return BattleUiElementType.TeammateInfo;
            }
            else
            {
                return BattleUiElementType.None;
            }
        }

        private BattleUiMovableElementData GetDefaultDataForUiElement(BattleUiElementType uiElementType)
        {
            if (uiElementType == BattleUiElementType.None) return null;

            // Initializing variables for creating data object
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.zero;

            OrientationType orientation = OrientationType.None;

            bool isFlippedHorizontally = false;
            bool isFlippedVertically = false;

            int handleSize = BattleUiMovableJoystickElement.HandleSizeDefault;

            // Rect variable so that we can do aspect ratio calculations
            Rect movableUiElementRect = Rect.zero;
            float aspectRatio = 0f;

            // Setting hardcoded default anchors (maybe there's a better way for this?)
            switch (uiElementType)
            {
                case BattleUiElementType.Timer:
                    if (_instantiatedTimer == null) return null;

                    anchorMin.x = 0.4f;
                    anchorMax.x = 0.6f;

                    anchorMin.y = 0.45f;
                    anchorMax.y = 0.55f;

                    movableUiElementRect = _instantiatedTimer.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.Diamonds:
                    if (_instantiatedDiamonds == null) return null;

                    anchorMin.x = 0.75f;
                    anchorMax.x = 0.95f;

                    anchorMin.y = 0.925f;
                    anchorMax.y = 0.975f;
                    movableUiElementRect = _instantiatedDiamonds.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.GiveUpButton:
                    if (_instantiatedGiveUpButton == null) return null;

                    anchorMin.x = 0.05f;
                    anchorMax.x = 0.25f;

                    anchorMin.y = 0.925f;
                    anchorMax.y = 0.975f;

                    movableUiElementRect = _instantiatedGiveUpButton.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.PlayerInfo:
                    if (_instantiatedPlayerInfo == null) return null;

                    anchorMin.x = 0.05f;
                    anchorMax.x = 0.35f;

                    anchorMin.y = 0.45f;
                    anchorMax.y = 0.55f;

                    orientation = OrientationType.Horizontal;

                    aspectRatio = _instantiatedPlayerInfo.HorizontalAspectRatio;
                    break;

                case BattleUiElementType.TeammateInfo:
                    if (_instantiatedTeammateInfo == null) return null;

                    anchorMin.x = 0.65f;
                    anchorMax.x = 0.95f;

                    anchorMin.y = 0.45f;
                    anchorMax.y = 0.55f;

                    orientation = OrientationType.Horizontal;

                    aspectRatio = _instantiatedTeammateInfo.HorizontalAspectRatio;
                    break;

                case BattleUiElementType.MoveJoystick:
                    if (_instantiatedMoveJoystick == null) return null;

                    anchorMin.x = 0.02f;
                    anchorMax.x = 0.4f;

                    anchorMin.y = 0f;
                    anchorMax.y = 0.4f;

                    movableUiElementRect = _instantiatedMoveJoystick.GetComponent<RectTransform>().rect;

                    // Toggling unsaved changes to save the default joystick data to fix issues with initializing in battle
                    _unsavedChanges = true;
                    break;

                case BattleUiElementType.RotateJoystick:
                    if (_instantiatedRotateJoystick == null) return null;

                    anchorMin.x = 0.6f;
                    anchorMax.x = 0.98f;

                    anchorMin.y = 0f;
                    anchorMax.y = 0.35f;

                    movableUiElementRect = _instantiatedRotateJoystick.GetComponent<RectTransform>().rect;

                    _unsavedChanges = true;
                    break;
            }

            // Calculating aspect ratio for movable elements (multiorientation elements have aspect ratios saved to serializefield)
            if (movableUiElementRect != Rect.zero) aspectRatio = movableUiElementRect.width / movableUiElementRect.height;

            // Calculating anchors
            Vector2 size = new();
            size.x = EditorRect.width * (anchorMax.x - anchorMin.x);
            size.y = size.x / aspectRatio;

            Vector2 pos = new(
                (anchorMin.x + anchorMax.x) * 0.5f * EditorRect.width,
                (anchorMax.y + anchorMin.y) * 0.5f * EditorRect.height
            );

            (anchorMin, anchorMax) = CalculateAnchors(size, pos);

            return new(uiElementType, anchorMin, anchorMax, 0, orientation, isFlippedHorizontally, isFlippedVertically, handleSize);
        }

        private BattleUiEditingComponent GetEditingComponent(BattleUiElementType uiElementType)
        {
            switch (uiElementType)
            {
                case BattleUiElementType.Timer:
                    return _timerEditingComponent;

                case BattleUiElementType.GiveUpButton:
                    return _giveUpButtonEditingComponent;

                case BattleUiElementType.Diamonds:
                    return _diamondsEditingComponent;

                case BattleUiElementType.PlayerInfo:
                    return _playerInfoEditingComponent;

                case BattleUiElementType.TeammateInfo:
                    return _teammateInfoEditingComponent;

                case BattleUiElementType.MoveJoystick:
                    return _moveJoystickEditingComponent;

                case BattleUiElementType.RotateJoystick:
                    return _rotateJoystickEditingComponent;

                default:
                    return null;
            }
        }

        private void OnUiElementEdited()
        {
            _unsavedChanges = !IsSavedDataSimilar();
            _grid.RemoveLineHighlight();
        }

        public void OnUiElementSelected(BattleUiEditingComponent newSelectedEditingComponent)
        {
            _optionsPopup.CloseOptionsPopup();
            _grid.RemoveLineHighlight();

            if (_currentlySelectedEditingComponent != null && _currentlySelectedEditingComponent != newSelectedEditingComponent)
            {
                _currentlySelectedEditingComponent.ShowControls(false);
            }

            _currentlySelectedEditingComponent = newSelectedEditingComponent;

            if (newSelectedEditingComponent != null)
            {
                int currentTransparency = _currentlySelectedEditingComponent.GetCurrentTransparency();
                _uiTransparencySlider.SetValueWithoutNotify(currentTransparency);
                _uiTransparencyInputField.SetTextWithoutNotify(currentTransparency.ToString());
                _uiTransparencyHolder.SetActive(true);
            }
            else
            {
                _uiTransparencyHolder.SetActive(false);
            }
        }

        public void UpdateInputFieldText(float value, TMP_InputField field)
        {
            bool isDecimal = field.contentType == TMP_InputField.ContentType.DecimalNumber;
            string valueString = isDecimal ? value.ToString("0.00") : value.ToString();
            field.SetTextWithoutNotify(valueString);
        }

        public void VerifyAndUpdateSliderValue(TMP_InputField field, Slider slider)
        {
            bool isDecimal = field.contentType == TMP_InputField.ContentType.DecimalNumber;

            if (int.TryParse(field.text, out int value))
            {
                int clampedValue = Math.Clamp(value, (int)slider.minValue, (int)slider.maxValue);
                string clampedValueString = isDecimal ? clampedValue.ToString("0.00") : clampedValue.ToString();

                field.SetTextWithoutNotify(clampedValueString);
                slider.SetValueWithoutNotify(clampedValue);
            }
            else
            {
                string minValueString = isDecimal ? slider.minValue.ToString("0.00") : slider.minValue.ToString();
                field.SetTextWithoutNotify(minValueString);
                slider.SetValueWithoutNotify(slider.minValue);
            }
        }

#if (UNITY_EDITOR)
        private Vector2 _previousScreenResolution;

        void Update()
        {
            if (_previousScreenResolution.x != Screen.width || _previousScreenResolution.y != Screen.height)
            {
                ScaleEditor();
                _previousScreenResolution = new(Screen.width, Screen.height);
            }
        }
#endif
    }
}
