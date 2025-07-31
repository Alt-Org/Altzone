using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Altzone.Scripts.BattleUiShared;
using OrientationType = Altzone.Scripts.BattleUiShared.BattleUiMultiOrientationElement.OrientationType;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;
using BattleMovementInputType = SettingsCarrier.BattleMovementInputType;
using BattleRotationInputType = SettingsCarrier.BattleRotationInputType;

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
        [SerializeField] public RectTransform _arenaImage;
        [SerializeField] private RectTransform _uiElementsHolder;
        [SerializeField] private GridController _grid;

        [Header("Options popup")]
        [SerializeField] private Button _optionsButton;
        [SerializeField] private OptionsPopup _optionsPopup;

        //[Header("Grid options")]
        //[SerializeField] private Toggle _showGridToggle;
        //[SerializeField] private Toggle _alignToGridToggle;
        //[SerializeField] private Toggle _incrementalScalingToggle;
        //[Space]
        //[SerializeField] private Slider _gridColumnsSlider;
        //[SerializeField] private TMP_InputField _gridColumnsInputField;
        //[Space]
        //[SerializeField] private Slider _gridRowsSlider;
        //[SerializeField] private TMP_InputField _gridRowsInputField;
        //[Space]
        //[SerializeField] private Slider _gridHueSlider;
        //[SerializeField] private TMP_InputField _gridHueInputField;
        //[Space]
        //[SerializeField] private Slider _gridTransparencySlider;
        //[SerializeField] private TMP_InputField _gridTransparencyInputField;

        //[Header("Input options")]
        //[SerializeField] private Toggle _swipeMovementToggle;
        //[SerializeField] private Toggle _pointAndClickMovementToggle;
        //[SerializeField] private Toggle _joystickMovementToggle;
        //[Space]
        //[SerializeField] private Toggle _twoFingerRotationToggle;
        //[SerializeField] private Toggle _swipeRotationToggle;
        //[SerializeField] private Toggle _joystickRotationToggle;
        //[SerializeField] private Toggle _gyroscopeRotationToggle;
        //[Space]
        //[SerializeField] private GameObject _swipeMinDistanceHolder;
        //[SerializeField] private Slider _swipeMinDistanceSlider;
        //[SerializeField] private TMP_InputField _swipeMinDistanceInputField;
        //[Space]
        //[SerializeField] private GameObject _swipeMaxDistanceHolder;
        //[SerializeField] private Slider _rotationSwipeMaxDistanceSlider;
        //[SerializeField] private TMP_InputField _rotationSwipeMaxDistanceInputField;
        //[Space]
        //[SerializeField] private GameObject _movementSwipeSensitivityHolder;
        //[SerializeField] private Slider _movementSwipeSensitivitySlider;
        //[SerializeField] private TMP_InputField _movementSwipeSensitivityInputField;
        //[Space]
        //[SerializeField] private GameObject _gyroscopeMinAngleHolder;
        //[SerializeField] private Slider _gyroscopeMinAngleSlider;
        //[SerializeField] private TMP_InputField _gyroscopeMinAngleInputField;

        //[Header("Arena options")]
        //[SerializeField] private Slider _arenaScaleSlider;
        //[SerializeField] private TMP_InputField _arenaScaleInputField;
        //[Space]
        //[SerializeField] private Slider _arenaPosXSlider;
        //[SerializeField] private TMP_InputField _arenaPosXInputField;
        //[Space]
        //[SerializeField] private Slider _arenaPosYSlider;
        //[SerializeField] private TMP_InputField _arenaPosYInputField;

        [Header("Save/reset popup")]
        [SerializeField] private GameObject _saveResetPopup;
        [SerializeField] private TMP_Text _popupText;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _noButton;

        [Header("BattleUi prefabs")]
        [SerializeField] private GameObject _editingComponent;
        [SerializeField] private GameObject _timer;
        [SerializeField] private GameObject _playerInfo;
        [SerializeField] private GameObject _diamonds;
        [SerializeField] private GameObject _giveUpButton;
        [SerializeField] private GameObject _joystick;

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
        public void CloseEditor()
        {
            _optionsPopup.CloseOptionsPopup();
            if (_unsavedChanges)
            {
                OnUiElementSelected(null);
                StartCoroutine(ShowSaveResetPopup(SaveChangesText, saveChanges =>
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
            if (_saveResetPopup.activeSelf) CloseSaveResetPopup();
            OnUiElementSelected(null);
        }

        private const string PlayerText = "Minä";
        private const string TeammateText = "Tiimikaveri";

        private const string SaveChangesText = "Tallenna muutokset?";

        private const string GridColumnLinesKey = "BattleUiEditorGridColumns";
        private const string GridRowLinesKey = "BattleUiEditorGridRows";
        private const string ShowGridKey = "BattleUiEditorShowGrid";
        private const string AlignToGridKey = "BattleUiEditorAlignToGrid";
        private const string IncrementalScalingKey = "BattleUiEditorIncScaling";
        private const string GridHueKey = "BattleUiEditorGridHue";
        private const string GridTransparencyKey = "BattleUiEditorGridTransparency";

        private const int GridRowLinesDefault = 39;
        private const int GridColumnLinesDefault = 19;
        private const int GridHueDefault = 33;
        private const int GridTransparencyDefault = 50;

        private const float GameAspectRatio = 9f / 16f;
        private const float TopButtonsHeight = 0.05f;

        private static RectTransform s_uiElementsHolder;

        private BattleUiMovableElement _instantiatedTimer;
        private BattleUiMultiOrientationElement _instantiatedPlayerInfo;
        private BattleUiMultiOrientationElement _instantiatedTeammateInfo;
        private BattleUiMovableElement _instantiatedDiamonds;
        private BattleUiMovableElement _instantiatedGiveUpButton;
        private BattleUiMovableElement _instantiatedMoveJoystick;
        private BattleUiMovableElement _instantiatedRotateJoystick;

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

            // Options dropdown listeners
            _optionsButton.onClick.AddListener(_optionsPopup.ToggleOptionsPopup);

            //// Show grid toggle listener
            //_optionsPopup._showGridToggle.onValueChanged.AddListener((value) =>
            //{
            //    _grid.SetShow(value);
            //    PlayerPrefs.SetInt(ShowGridKey, value ? 1 : 0);
            //});

            //// Grid columns listeners
            //_optionsPopup._gridColumnsSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _optionsPopup._gridColumnsInputField);
            //    UpdateGridColumnLines();
            //});
            //_optionsPopup._gridColumnsInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_optionsPopup._gridColumnsInputField, _optionsPopup._gridColumnsSlider);
            //    UpdateGridColumnLines();
            //});

            //// Grid rows listeners
            //_optionsPopup._gridRowsSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _optionsPopup._gridRowsInputField);
            //    UpdateGridRowLines();
            //});
            //_optionsPopup._gridRowsInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_optionsPopup._gridRowsInputField, _optionsPopup._gridRowsSlider);
            //    UpdateGridRowLines();
            //});

            //// Grid hue listeners
            //_optionsPopup._gridHueSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _optionsPopup._gridHueInputField);
            //    _grid.SetGridHue(value);
            //    PlayerPrefs.SetInt(GridHueKey, (int)value);
            //});
            //_optionsPopup._gridHueInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_optionsPopup._gridHueInputField, _optionsPopup._gridHueSlider);
            //    _grid.SetGridHue(_optionsPopup._gridHueSlider.value);
            //    PlayerPrefs.SetInt(GridHueKey, (int)_optionsPopup._gridHueSlider.value);
            //});

            //// Grid transparency listeners
            //_optionsPopup._gridTransparencySlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _optionsPopup._gridTransparencyInputField);
            //    _grid.SetGridTransparency(value);
            //    PlayerPrefs.SetInt(GridTransparencyKey, (int)value);
            //});
            //_optionsPopup._gridTransparencyInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_optionsPopup._gridTransparencyInputField, _optionsPopup._gridTransparencySlider);
            //    _grid.SetGridTransparency(_optionsPopup._gridTransparencySlider.value);
            //    PlayerPrefs.SetInt(GridTransparencyKey, (int)_optionsPopup._gridTransparencySlider.value);
            //});

            //// Input options listeners
            //_swipeMovementToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(BattleMovementInputType.Swipe, BattleRotationInputType.TwoFinger); });
            //_pointAndClickMovementToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(BattleMovementInputType.PointAndClick, BattleRotationInputType.TwoFinger); });
            //_joystickMovementToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(BattleMovementInputType.Joystick, BattleRotationInputType.Joystick); });

            //_twoFingerRotationToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, BattleRotationInputType.TwoFinger); });
            //_swipeRotationToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, BattleRotationInputType.Swipe); });
            //_joystickRotationToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, BattleRotationInputType.Joystick); });
            //_gyroscopeRotationToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, BattleRotationInputType.Gyroscope); });

            //_swipeMinDistanceSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _swipeMinDistanceInputField);
            //    SettingsCarrier.Instance.BattleSwipeMinDistance = value;
            //});
            //_swipeMinDistanceInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_swipeMinDistanceInputField, _swipeMinDistanceSlider);
            //    SettingsCarrier.Instance.BattleSwipeMinDistance = _swipeMinDistanceSlider.value;
            //});

            //_rotationSwipeMaxDistanceSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _rotationSwipeMaxDistanceInputField);
            //    SettingsCarrier.Instance.BattleSwipeMaxDistance = value;
            //});
            //_rotationSwipeMaxDistanceInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_rotationSwipeMaxDistanceInputField, _rotationSwipeMaxDistanceSlider);
            //    SettingsCarrier.Instance.BattleSwipeMaxDistance = _rotationSwipeMaxDistanceSlider.value;
            //});

            //_movementSwipeSensitivitySlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _movementSwipeSensitivityInputField);
            //    SettingsCarrier.Instance.BattleSwipeSensitivity = value;
            //});
            //_movementSwipeSensitivityInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_movementSwipeSensitivityInputField, _movementSwipeSensitivitySlider);
            //    SettingsCarrier.Instance.BattleSwipeSensitivity = _movementSwipeSensitivitySlider.value;
            //});

            //_gyroscopeMinAngleSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _gyroscopeMinAngleInputField);
            //    SettingsCarrier.Instance.BattleGyroMinAngle = value;
            //});
            //_gyroscopeMinAngleInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_gyroscopeMinAngleInputField, _gyroscopeMinAngleSlider);
            //    SettingsCarrier.Instance.BattleGyroMinAngle = _gyroscopeMinAngleSlider.value;
            //});

            //// Arena scale listeners
            //_arenaScaleSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _arenaScaleInputField);
            //    SettingsCarrier.Instance.BattleArenaScale = (int)value;
            //    UpdateArena();
            //});
            //_arenaScaleInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_arenaScaleInputField, _arenaScaleSlider);
            //    SettingsCarrier.Instance.BattleArenaScale = (int)_arenaScaleSlider.value;
            //    UpdateArena();
            //});

            //// Arena pos x listeners
            //_arenaPosXSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _arenaPosXInputField);
            //    SettingsCarrier.Instance.BattleArenaPosX = (int)value;
            //    UpdateArena();
            //});
            //_arenaPosXInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_arenaPosXInputField, _arenaPosXSlider);
            //    SettingsCarrier.Instance.BattleArenaPosX = (int)_arenaPosXSlider.value;
            //    UpdateArena();
            //});

            //// Arena pos y listeners
            //_arenaPosYSlider.onValueChanged.AddListener((value) =>
            //{
            //    UpdateInputFieldText(value, _arenaPosYInputField);
            //    SettingsCarrier.Instance.BattleArenaPosY = (int)value;
            //    UpdateArena();
            //});
            //_arenaPosYInputField.onValueChanged.AddListener((value) =>
            //{
            //    VerifyAndUpdateSliderValue(_arenaPosYInputField, _arenaPosYSlider);
            //    SettingsCarrier.Instance.BattleArenaPosY = (int)_arenaPosYSlider.value;
            //    UpdateArena();
            //});
        }

        private void Start()
        {
            // Initializing options popup
            _optionsPopup.Initialize(this);

            //// Loading grid settings. Grid settings are saved locally from this script because they aren't accessed anywhere else.
            //_optionsPopup._gridColumnsSlider.value = PlayerPrefs.GetInt(GridColumnLinesKey, GridColumnLinesDefault);
            //_optionsPopup._gridRowsSlider.value = PlayerPrefs.GetInt(GridRowLinesKey, GridRowLinesDefault);
            //_optionsPopup._gridHueSlider.value = PlayerPrefs.GetInt(GridHueKey, GridHueDefault);
            //_optionsPopup._gridTransparencySlider.value = PlayerPrefs.GetInt(GridTransparencyKey, GridTransparencyDefault);

            //_optionsPopup._showGridToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(ShowGridKey, 1) == 1);
            //_optionsPopup._alignToGridToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(AlignToGridKey, 1) == 1);
            //_optionsPopup._incrementalScalingToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(IncrementalScalingKey, 1) == 1);

            //// Initializing grid
            //_grid.SetRowLines((int)_optionsPopup._gridRowsSlider.value);
            //_grid.SetColumnLines((int)_optionsPopup._gridColumnsSlider.value);
            //_grid.SetShow(_optionsPopup._showGridToggle.isOn);

            //// Loading saved input settings
            //switch (SettingsCarrier.Instance.BattleMovementInput)
            //{
            //    case BattleMovementInputType.Swipe:
            //        _optionsPopup._swipeMovementToggle.SetIsOnWithoutNotify(true);
            //        break;
            //    case BattleMovementInputType.PointAndClick:
            //        _optionsPopup._pointAndClickMovementToggle.SetIsOnWithoutNotify(true);
            //        break;
            //    case BattleMovementInputType.Joystick:
            //        _optionsPopup._joystickMovementToggle.SetIsOnWithoutNotify(true);
            //        break;
            //}

            //_optionsPopup.UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, SettingsCarrier.Instance.BattleRotationInput);

            //float swipeMinDistance = SettingsCarrier.Instance.BattleSwipeMinDistance;
            //_optionsPopup._swipeMinDistanceSlider.value = swipeMinDistance;
            //UpdateInputFieldText(swipeMinDistance, _optionsPopup._swipeMinDistanceInputField);

            //float swipeMaxDistance = SettingsCarrier.Instance.BattleSwipeMaxDistance;
            //_optionsPopup._rotationSwipeMaxDistanceSlider.value = swipeMaxDistance;
            //UpdateInputFieldText(swipeMaxDistance, _optionsPopup._rotationSwipeMaxDistanceInputField);

            //float swipeSensitivity = SettingsCarrier.Instance.BattleSwipeSensitivity;
            //_optionsPopup._movementSwipeSensitivitySlider.value = swipeSensitivity;
            //UpdateInputFieldText(swipeSensitivity, _optionsPopup._movementSwipeSensitivityInputField);

            //float gyroMinAngle = SettingsCarrier.Instance.BattleGyroMinAngle;
            //_optionsPopup._gyroscopeMinAngleSlider.value = gyroMinAngle;
            //UpdateInputFieldText(gyroMinAngle, _optionsPopup._gyroscopeMinAngleInputField);

            //// Loading saved arena settings. Setting slider value will invoke the listeners added in Awake so the input fields will be updated as well.
            //_optionsPopup._arenaScaleSlider.value = SettingsCarrier.Instance.BattleArenaScale;
            //_optionsPopup._arenaPosXSlider.value = SettingsCarrier.Instance.BattleArenaPosX;
            //_optionsPopup._arenaPosYSlider.value = SettingsCarrier.Instance.BattleArenaPosY;
        }

        private void OnDestroy()
        {
            // Removing close and save button listeners
            _closeButton.onClick.RemoveAllListeners();
            _saveButton.onClick.RemoveAllListeners();

            // Removing preview mode listeners
            _previewButton.onClick.RemoveAllListeners();
            _previewModeTouchDetector.onClick.RemoveAllListeners();

            // Removing save changes popup listeners
            _okButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();

            // Removing editing component listeners
            foreach (BattleUiEditingComponent editingComponent in _editingComponents)
            {
                editingComponent.OnUiElementEdited -= OnUiElementEdited;
                editingComponent.OnUiElementSelected -= OnUiElementSelected;
                editingComponent.OnGridSnap -= _grid.HighlightLines;
            }

            // Removing options button listeners
            _optionsButton.onClick.RemoveAllListeners();

            //// Removing grid listeners
            //_gridColumnsSlider.onValueChanged.RemoveAllListeners();
            //_gridColumnsInputField.onValueChanged.RemoveAllListeners();

            //_gridRowsSlider.onValueChanged.RemoveAllListeners();
            //_gridRowsInputField.onValueChanged.RemoveAllListeners();

            //_gridHueSlider.onValueChanged.RemoveAllListeners();
            //_gridHueInputField.onValueChanged.RemoveAllListeners();

            //_gridTransparencySlider.onValueChanged.RemoveAllListeners();
            //_gridTransparencyInputField.onValueChanged.RemoveAllListeners();

            //_showGridToggle.onValueChanged.RemoveAllListeners();
            //_incrementalScalingToggle.onValueChanged.RemoveAllListeners();
            //_alignToGridToggle.onValueChanged.RemoveAllListeners();

            //// Removing input options listeners
            //_swipeMovementToggle.onValueChanged.RemoveAllListeners();
            //_pointAndClickMovementToggle.onValueChanged.RemoveAllListeners();
            //_joystickMovementToggle.onValueChanged.RemoveAllListeners();

            //_twoFingerRotationToggle.onValueChanged.RemoveAllListeners();
            //_swipeRotationToggle.onValueChanged.RemoveAllListeners();
            //_joystickRotationToggle.onValueChanged.RemoveAllListeners();
            //_gyroscopeRotationToggle.onValueChanged.RemoveAllListeners();

            //_swipeMinDistanceSlider.onValueChanged.RemoveAllListeners();
            //_swipeMinDistanceInputField.onValueChanged.RemoveAllListeners();

            //_rotationSwipeMaxDistanceSlider.onValueChanged.RemoveAllListeners();
            //_rotationSwipeMaxDistanceInputField.onValueChanged.RemoveAllListeners();

            //_movementSwipeSensitivitySlider.onValueChanged.RemoveAllListeners();
            //_movementSwipeSensitivityInputField.onValueChanged.RemoveAllListeners();

            //_gyroscopeMinAngleSlider.onValueChanged.RemoveAllListeners();
            //_gyroscopeMinAngleInputField.onValueChanged.RemoveAllListeners();

            //// Removing arena scale listeners
            //_arenaScaleSlider.onValueChanged.RemoveAllListeners();
            //_arenaScaleInputField.onValueChanged.RemoveAllListeners();

            //_arenaPosXSlider.onValueChanged.RemoveAllListeners();
            //_arenaPosXInputField.onValueChanged.RemoveAllListeners();

            //_arenaPosYSlider.onValueChanged.RemoveAllListeners();
            //_arenaPosYInputField.onValueChanged.RemoveAllListeners();
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

        public IEnumerator ShowSaveResetPopup(string message, Action<bool?> callback)
        {
            _popupText.text = message;
            _saveResetPopup.SetActive(true);

            _okButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();

            bool? saveChanges = null;

            _okButton.onClick.AddListener(() => saveChanges = true);
            _noButton.onClick.AddListener(() => saveChanges = false);

            yield return new WaitUntil(() => saveChanges.HasValue || !_saveResetPopup.activeSelf);

            if (_saveResetPopup.activeSelf) CloseSaveResetPopup();

            callback(saveChanges);
        }

        private void CloseSaveResetPopup()
        {
            _saveResetPopup.SetActive(false);
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
                case BattleUiElementType.MoveJoystick:
                case BattleUiElementType.RotateJoystick:
                    uiElementPrefab = _joystick;
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
            _optionsPopup._alignToGridToggle.onValueChanged.AddListener((value)=>
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

                case BattleUiElementType.MoveJoystick:
                case BattleUiElementType.RotateJoystick:
                    BattleUiJoystickIconSetter iconSetter = uiElementGameObject.GetComponent<BattleUiJoystickIconSetter>();
                    if (iconSetter != null) iconSetter.SetIcon(uiElementType);
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
                    anchorMax.y = 0.2f;

                    movableUiElementRect = _instantiatedMoveJoystick.GetComponent<RectTransform>().rect;

                    // Toggling unsaved changes to save the default joystick data to fix issues with initializing in battle
                    _unsavedChanges = true;
                    break;

                case BattleUiElementType.RotateJoystick:
                    if (_instantiatedRotateJoystick == null) return null;

                    anchorMin.x = 0.6f;
                    anchorMax.x = 0.98f;

                    anchorMin.y = 0f;
                    anchorMax.y = 0.15f;

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

        //private void UpdateGridColumnLines()
        //{
        //    int columns = (int)_optionsPopup._gridColumnsSlider.value;
        //    if (_grid.SetColumnLines(columns)) PlayerPrefs.SetInt(GridColumnLinesKey, columns);
        //}

        //private void UpdateGridRowLines()
        //{
        //    int rows = (int)_optionsPopup._gridRowsSlider.value;
        //    if (_grid.SetRowLines(rows)) PlayerPrefs.SetInt(GridRowLinesKey, rows);
        //}

        //private void UpdateArena()
        //{
        //    float screenAspectRatio = Screen.width / (float)Screen.height;

        //    // For some reason the editor has different aspect ratio calculated from rect size in local space than in world space because of the editor scaling
        //    // Getting editor corners in world space
        //    Vector3[] editorCorners = new Vector3[4];
        //    EditorRectTransform.GetWorldCorners(editorCorners);

        //    // Calculating world space size and aspect ratio
        //    Vector2 editorWorldSize = new(editorCorners[(int)CornerType.TopRight].x - editorCorners[(int)CornerType.TopLeft].x,
        //               editorCorners[(int)CornerType.TopRight].y - editorCorners[(int)CornerType.BottomRight].y);
        //    float editorWorldAspectRatio = editorWorldSize.x / editorWorldSize.y;

        //    // Calculating a height for the editor from the world aspect ratio so that it works in calculations
        //    float editorAspectRatioHeight = EditorRect.width / editorWorldAspectRatio;

        //    // Calculating arena scale.
        //    // If phone aspect ratio is same or thinner than the game aspect ratio we calculate arena width and height based on
        //    // editor width, but if it's thicker we calculate based on height so that the arena won't overlap or be too small.
        //    float arenaWidth;
        //    float arenaHeight; 
        //    if (screenAspectRatio <= GameAspectRatio)
        //    {
        //        arenaWidth = _arenaScaleSlider.value * 0.01f * EditorRect.width;
        //        arenaHeight = arenaWidth / GameAspectRatio;
        //    }
        //    else
        //    {
        //        arenaHeight = _arenaScaleSlider.value * 0.01f * editorAspectRatioHeight;
        //        arenaWidth = arenaHeight * GameAspectRatio;
        //    }

        //    // Calculating arena position
        //    Vector2 position = Vector2.zero;
        //    position.x += _arenaPosXSlider.value * 0.01f * (EditorRect.width - arenaWidth);
        //    position.y += (100f - _arenaPosYSlider.value) * 0.01f * (editorAspectRatioHeight - arenaHeight);

        //    // Calculating arena anchors
        //    Vector2 anchorMin = Vector2.zero;
        //    Vector2 anchorMax = Vector2.zero;

        //    anchorMin.x = position.x / EditorRect.width;
        //    anchorMax.x = (position.x + arenaWidth) / EditorRect.width;

        //    anchorMin.y = position.y / editorAspectRatioHeight;
        //    anchorMax.y = (position.y + arenaHeight) / editorAspectRatioHeight;

        //    // Setting arena anchors
        //    _arenaImage.anchorMin = anchorMin;
        //    _arenaImage.anchorMax = anchorMax;

        //    _arenaImage.offsetMin = Vector2.zero;
        //    _arenaImage.offsetMax = Vector2.zero;
        //}

        //private void UpdateInputSettings(BattleMovementInputType movementType, BattleRotationInputType rotationType)
        //{
        //    // Setting input values to settings carrier
        //    SettingsCarrier.Instance.BattleMovementInput = movementType;
        //    SettingsCarrier.Instance.BattleRotationInput = rotationType;

        //    // If joystick movement was selected instantianting the joysticks if they are not yet instantiated
        //    if (movementType == BattleMovementInputType.Joystick)
        //    {
        //        if (_instantiatedMoveJoystick == null)
        //        {
        //            _instantiatedMoveJoystick = InstantiateBattleUiElement(BattleUiElementType.MoveJoystick).GetComponent<BattleUiMovableElement>();
        //            SetDataToUiElement(_instantiatedMoveJoystick);
        //        }

        //        if (_instantiatedRotateJoystick == null)
        //        {
        //            _instantiatedRotateJoystick = InstantiateBattleUiElement(BattleUiElementType.RotateJoystick).GetComponent<BattleUiMovableElement>();
        //            SetDataToUiElement(_instantiatedRotateJoystick);
        //        }
        //    }

        //    // Toggling rotation toggles isOn based on rotation type and visibility based on movement type
        //    _twoFingerRotationToggle.SetIsOnWithoutNotify(rotationType == BattleRotationInputType.TwoFinger);
        //    _twoFingerRotationToggle.gameObject.SetActive(movementType == BattleMovementInputType.Swipe || movementType == BattleMovementInputType.PointAndClick);

        //    _swipeRotationToggle.SetIsOnWithoutNotify(rotationType == BattleRotationInputType.Swipe);
        //    _swipeRotationToggle.gameObject.SetActive(movementType == BattleMovementInputType.PointAndClick);

        //    _joystickRotationToggle.SetIsOnWithoutNotify(rotationType == BattleRotationInputType.Joystick);
        //    _joystickRotationToggle.gameObject.SetActive(movementType == BattleMovementInputType.Joystick);

        //    _gyroscopeRotationToggle.SetIsOnWithoutNotify(rotationType == BattleRotationInputType.Gyroscope);

        //    // Setting visibility for the swipe and gyroscope additional options
        //    _swipeMinDistanceHolder.SetActive(movementType == BattleMovementInputType.Swipe || rotationType == BattleRotationInputType.Swipe);
        //    _swipeMaxDistanceHolder.SetActive(rotationType == BattleRotationInputType.Swipe);
        //    _movementSwipeSensitivityHolder.SetActive(movementType == BattleMovementInputType.Swipe);
        //    _gyroscopeMinAngleHolder.SetActive(rotationType == BattleRotationInputType.Gyroscope);

        //    // Setting visibility to joysticks
        //    if (_instantiatedMoveJoystick != null) _instantiatedMoveJoystick.gameObject.SetActive(movementType == BattleMovementInputType.Joystick);
        //    if (_instantiatedRotateJoystick != null) _instantiatedRotateJoystick.gameObject.SetActive(rotationType == BattleRotationInputType.Joystick);
        //}

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
