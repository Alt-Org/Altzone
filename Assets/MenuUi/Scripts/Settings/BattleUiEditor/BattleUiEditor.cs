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

using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Handles the functionality for BattleUiEditor prefab in Settings.
    /// </summary>
    public class BattleUiEditor : MonoBehaviour
    {
        [Header("Editor GameObject references")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _saveButton;
        [Space]
        [SerializeField] private RectTransform _arenaImage;
        [SerializeField] private RectTransform _uiElementsHolder;
        [SerializeField] private GridController _grid;

        [Header("Options popup")]
        [SerializeField] private Button _optionsButton;
        [SerializeField] private GameObject _optionsContents;
        [SerializeField] private Button _resetButton;

        [Header("Grid options")]
        [SerializeField] private Toggle _showGridToggle;
        [SerializeField] private Toggle _alignToGridToggle;
        [SerializeField] private Toggle _incrementalScalingToggle;
        [Space]
        [SerializeField] private Slider _gridColumnsSlider;
        [SerializeField] private TMP_InputField _gridColumnsInputField;
        [Space]
        [SerializeField] private Slider _gridRowsSlider;
        [SerializeField] private TMP_InputField _gridRowsInputField;
        [Space]
        [SerializeField] private Slider _gridHueSlider;
        [SerializeField] private TMP_InputField _gridHueInputField;
        [Space]
        [SerializeField] private Slider _gridTransparencySlider;
        [SerializeField] private TMP_InputField _gridTransparencyInputField;

        [Header("Input options")]
        [SerializeField] private Button _inputSelectorLeftButton;
        [SerializeField] private Button _inputSelectorRightButton;
        [SerializeField] private TMP_Text _inputSelectorLabel;
        [Space]
        [SerializeField] private GameObject _swipeMinDistanceHolder;
        [SerializeField] private Slider _swipeMinDistanceSlider;
        [SerializeField] private TMP_InputField _swipeMinDistanceInputField;
        [Space]
        [SerializeField] private GameObject _swipeMaxDistanceHolder;
        [SerializeField] private Slider _rotationSwipeMaxDistanceSlider;
        [SerializeField] private TMP_InputField _rotationSwipeMaxDistanceInputField;
        [Space]
        [SerializeField] private GameObject _movementSwipeSensitivityHolder;
        [SerializeField] private Slider _movementSwipeSensitivitySlider;
        [SerializeField] private TMP_InputField _movementSwipeSensitivityInputField;
        [Space]
        [SerializeField] private Toggle _rotationGyroscopeOverrideToggle;
        [Space]
        [SerializeField] private GameObject _gyroscopeMinAngleHolder;
        [SerializeField] private Slider _gyroscopeMinAngleSlider;
        [SerializeField] private TMP_InputField _gyroscopeMinAngleInputField;

        [Header("Arena options")]
        [SerializeField] private Slider _arenaScaleSlider;
        [SerializeField] private TMP_InputField _arenaScaleInputField;
        [Space]
        [SerializeField] private Slider _arenaPosXSlider;
        [SerializeField] private TMP_InputField _arenaPosXInputField;
        [Space]
        [SerializeField] private Slider _arenaPosYSlider;
        [SerializeField] private TMP_InputField _arenaPosYInputField;

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

        public static float ScreenSpaceRatio => Screen.width / EditorRect.width;

        public static Rect EditorRect;
        public static RectTransform EditorRectTransform;

        /// <summary>
        /// Calculate anchors based on Ui element size position and offset.
        /// </summary>
        /// <param name="size">Size of the Ui element.</param>
        /// <param name="pos">Ui element position.</param>
        /// <param name="offset">Optional offset for the anchors.</param>
        /// <returns>Two Vector2, anchorMin and anchorMax.</returns>
        public static (Vector2 anchorMin, Vector2 anchorMax) CalculateAnchors(Vector2 size, Vector2 pos, float offset = 0f)
        {
            // For some reason the calculation didn't work with screen space ratio when called from BattleUiEditingComponent which also needs the offset, so added a check.
            float uiHolderWidth = offset == 0f ? EditorRect.width * ScreenSpaceRatio : EditorRect.width;
            float uiHolderHeight = offset == 0f ? EditorRect.height * ScreenSpaceRatio : EditorRect.height;

            // Calculating anchors
            float anchorXMin = Mathf.Clamp01((pos.x - size.x * 0.5f) / uiHolderWidth + offset);
            float anchorXMax = Mathf.Clamp01((pos.x + size.x * 0.5f) / uiHolderWidth + offset);

            float anchorYMin = Mathf.Clamp01((pos.y - size.y * 0.5f) / uiHolderHeight + offset);
            float anchorYMax = Mathf.Clamp01((pos.y + size.y * 0.5f) / uiHolderHeight + offset);

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
            CloseOptionsDropdown();
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

        private enum InputCombinationType
        {
            SwipeTwoFinger,
            PointAndClickSwipe,
            Joysticks,
        }

        private const string PlayerText = "Min‰";
        private const string TeammateText = "Tiimikaveri";

        private const string SaveChangesText = "Tallenna muutokset?";
        private const string ResetChangesText = "Palauta UI-elementtien oletusasettelu?";

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
        private InputCombinationType _currentInputCombinationType;

        private void Awake()
        {
            EditorRectTransform = GetComponent<RectTransform>();
            EditorRect = EditorRectTransform.rect;

            // Close and save button listeners
            _closeButton.onClick.AddListener(CloseEditor);
            _saveButton.onClick.AddListener(SaveChanges);

            // Options dropdown listeners
            _optionsButton.onClick.AddListener(ToggleOptionsDropdown);

            // Reset button listener
            _resetButton.onClick.AddListener(OnResetButtonClicked);

            // Show grid toggle listener
            _showGridToggle.onValueChanged.AddListener((value) =>
            {
                _grid.SetShow(value);
                PlayerPrefs.SetInt(ShowGridKey, value ? 1 : 0);
            });

            // Grid columns listeners
            _gridColumnsSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _gridColumnsInputField);
                UpdateGridColumnLines();
            });
            _gridColumnsInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_gridColumnsInputField, _gridColumnsSlider);
                UpdateGridColumnLines();
            });

            // Grid rows listeners
            _gridRowsSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _gridRowsInputField);
                UpdateGridRowLines();
            });
            _gridRowsInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_gridRowsInputField, _gridRowsSlider);
                UpdateGridRowLines();
            });

            // Grid hue listeners
            _gridHueSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _gridHueInputField);
                _grid.SetGridHue(value);
                PlayerPrefs.SetInt(GridHueKey, (int)value);
            });
            _gridHueInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_gridHueInputField, _gridHueSlider);
                _grid.SetGridHue(_gridHueSlider.value);
                PlayerPrefs.SetInt(GridHueKey, (int)_gridHueSlider.value);
            });

            // Grid transparency listeners
            _gridTransparencySlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _gridTransparencyInputField);
                _grid.SetGridTransparency(value);
                PlayerPrefs.SetInt(GridTransparencyKey, (int)value);
            });
            _gridTransparencyInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_gridTransparencyInputField, _gridTransparencySlider);
                _grid.SetGridTransparency(_gridTransparencySlider.value);
                PlayerPrefs.SetInt(GridTransparencyKey, (int)_gridTransparencySlider.value);
            });

            // Input options listeners
            _inputSelectorLeftButton.onClick.AddListener(() => PreviousInputCombination());
            _inputSelectorRightButton.onClick.AddListener(() => NextInputCombination());
            _rotationGyroscopeOverrideToggle.onValueChanged.AddListener((value) => { UpdateInputSettings(); });

            _swipeMinDistanceSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _swipeMinDistanceInputField);
                SettingsCarrier.Instance.BattleSwipeMinDistance = value;
            });
            _swipeMinDistanceInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_swipeMinDistanceInputField, _swipeMinDistanceSlider);
                SettingsCarrier.Instance.BattleSwipeMinDistance = _swipeMinDistanceSlider.value;
            });

            _rotationSwipeMaxDistanceSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _rotationSwipeMaxDistanceInputField);
                SettingsCarrier.Instance.BattleSwipeMaxDistance = value;
            });
            _rotationSwipeMaxDistanceInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_rotationSwipeMaxDistanceInputField, _rotationSwipeMaxDistanceSlider);
                SettingsCarrier.Instance.BattleSwipeMaxDistance = _rotationSwipeMaxDistanceSlider.value;
            });

            _movementSwipeSensitivitySlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _movementSwipeSensitivityInputField);
                SettingsCarrier.Instance.BattleSwipeSensitivity = value;
            });
            _movementSwipeSensitivityInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_movementSwipeSensitivityInputField, _movementSwipeSensitivitySlider);
                SettingsCarrier.Instance.BattleSwipeSensitivity = _movementSwipeSensitivitySlider.value;
            });

            _gyroscopeMinAngleSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _gyroscopeMinAngleInputField);
                SettingsCarrier.Instance.BattleGyroMinAngle = value;
            });
            _gyroscopeMinAngleInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_gyroscopeMinAngleInputField, _gyroscopeMinAngleSlider);
                SettingsCarrier.Instance.BattleGyroMinAngle = _gyroscopeMinAngleSlider.value;
            });

            // Arena scale listeners
            _arenaScaleSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _arenaScaleInputField);
                SettingsCarrier.Instance.BattleArenaScale = (int)value;
                UpdateArena();
            });
            _arenaScaleInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_arenaScaleInputField, _arenaScaleSlider);
                SettingsCarrier.Instance.BattleArenaScale = (int)_arenaScaleSlider.value;
                UpdateArena();
            });

            // Arena pos x listeners
            _arenaPosXSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _arenaPosXInputField);
                SettingsCarrier.Instance.BattleArenaPosX = (int)value;
                UpdateArena();
            });
            _arenaPosXInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_arenaPosXInputField, _arenaPosXSlider);
                SettingsCarrier.Instance.BattleArenaPosX = (int)_arenaPosXSlider.value;
                UpdateArena();
            });

            // Arena pos y listeners
            _arenaPosYSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _arenaPosYInputField);
                SettingsCarrier.Instance.BattleArenaPosY = (int)value;
                UpdateArena();
            });
            _arenaPosYInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_arenaPosYInputField, _arenaPosYSlider);
                SettingsCarrier.Instance.BattleArenaPosY = (int)_arenaPosYSlider.value;
                UpdateArena();
            });
        }

        private void Start()
        {
            // Loading grid settings. Grid settings are saved locally from this script because they aren't accessed anywhere else.
            _gridColumnsSlider.value = PlayerPrefs.GetInt(GridColumnLinesKey, GridColumnLinesDefault);
            _gridRowsSlider.value = PlayerPrefs.GetInt(GridRowLinesKey, GridRowLinesDefault);
            _gridHueSlider.value = PlayerPrefs.GetInt(GridHueKey, GridHueDefault);
            _gridTransparencySlider.value = PlayerPrefs.GetInt(GridTransparencyKey, GridTransparencyDefault);

            _showGridToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(ShowGridKey, 1) == 1);
            _alignToGridToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(AlignToGridKey, 1) == 1);
            _incrementalScalingToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(IncrementalScalingKey, 1) == 1);

            // Initializing grid
            _grid.SetRowLines((int)_gridRowsSlider.value);
            _grid.SetColumnLines((int)_gridColumnsSlider.value);
            _grid.SetShow(_showGridToggle.isOn);

            // Loading saved input settings
            _rotationGyroscopeOverrideToggle.SetIsOnWithoutNotify(SettingsCarrier.Instance.BattleRotationInput == BattleRotationInputType.Gyroscope);

            switch (SettingsCarrier.Instance.BattleMovementInput)
            {
                case BattleMovementInputType.PointAndClick:
                    _currentInputCombinationType = InputCombinationType.PointAndClickSwipe;
                    break;
                case BattleMovementInputType.Swipe:
                    _currentInputCombinationType = InputCombinationType.SwipeTwoFinger;
                    break;
                case BattleMovementInputType.Joystick:
                    _currentInputCombinationType = InputCombinationType.Joysticks;
                    break;
            }

            UpdateInputSettings();

            float swipeMinDistance = SettingsCarrier.Instance.BattleSwipeMinDistance;
            _swipeMinDistanceSlider.value = swipeMinDistance;
            UpdateInputFieldText(swipeMinDistance, _swipeMinDistanceInputField);

            float swipeMaxDistance = SettingsCarrier.Instance.BattleSwipeMaxDistance;
            _rotationSwipeMaxDistanceSlider.value = swipeMaxDistance;
            UpdateInputFieldText(swipeMaxDistance, _rotationSwipeMaxDistanceInputField);

            float swipeSensitivity = SettingsCarrier.Instance.BattleSwipeSensitivity;
            _movementSwipeSensitivitySlider.value = swipeSensitivity;
            UpdateInputFieldText(swipeSensitivity, _movementSwipeSensitivityInputField);

            float gyroMinAngle = SettingsCarrier.Instance.BattleGyroMinAngle;
            _gyroscopeMinAngleSlider.value = gyroMinAngle;
            UpdateInputFieldText(gyroMinAngle, _gyroscopeMinAngleInputField);
            
            // Loading saved arena settings. Setting slider vlaue will invoke the listeners added in Awake so the input fields will be updated as well.
            _arenaScaleSlider.value = SettingsCarrier.Instance.BattleArenaScale;
            _arenaPosXSlider.value = SettingsCarrier.Instance.BattleArenaPosX;
            _arenaPosYSlider.value = SettingsCarrier.Instance.BattleArenaPosY;
        }

        private void OnDestroy()
        {
            // Removing close and save button listeners
            _closeButton.onClick.RemoveAllListeners();
            _saveButton.onClick.RemoveAllListeners();

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

            // Removing options dropdown listeners
            _optionsButton.onClick.RemoveAllListeners();
            _resetButton.onClick.RemoveAllListeners();

            // Removing grid listeners
            _gridColumnsSlider.onValueChanged.RemoveAllListeners();
            _gridColumnsInputField.onValueChanged.RemoveAllListeners();

            _gridRowsSlider.onValueChanged.RemoveAllListeners();
            _gridRowsInputField.onValueChanged.RemoveAllListeners();

            _gridHueSlider.onValueChanged.RemoveAllListeners();
            _gridHueInputField.onValueChanged.RemoveAllListeners();

            _gridTransparencySlider.onValueChanged.RemoveAllListeners();
            _gridTransparencyInputField.onValueChanged.RemoveAllListeners();

            _showGridToggle.onValueChanged.RemoveAllListeners();
            _incrementalScalingToggle.onValueChanged.RemoveAllListeners();
            _alignToGridToggle.onValueChanged.RemoveAllListeners();

            // Removing input options listeners
            _inputSelectorLeftButton.onClick.RemoveAllListeners();
            _inputSelectorRightButton.onClick.RemoveAllListeners();
            _rotationGyroscopeOverrideToggle.onValueChanged.RemoveAllListeners();

            _swipeMinDistanceSlider.onValueChanged.RemoveAllListeners();
            _swipeMinDistanceInputField.onValueChanged.RemoveAllListeners();

            _rotationSwipeMaxDistanceSlider.onValueChanged.RemoveAllListeners();
            _rotationSwipeMaxDistanceInputField.onValueChanged.RemoveAllListeners();

            _movementSwipeSensitivitySlider.onValueChanged.RemoveAllListeners();
            _movementSwipeSensitivityInputField.onValueChanged.RemoveAllListeners();

            _gyroscopeMinAngleSlider.onValueChanged.RemoveAllListeners();
            _gyroscopeMinAngleInputField.onValueChanged.RemoveAllListeners();

            // Removing arena scale listeners
            _arenaScaleSlider.onValueChanged.RemoveAllListeners();
            _arenaScaleInputField.onValueChanged.RemoveAllListeners();

            _arenaPosXSlider.onValueChanged.RemoveAllListeners();
            _arenaPosXInputField.onValueChanged.RemoveAllListeners();

            _arenaPosYSlider.onValueChanged.RemoveAllListeners();
            _arenaPosYInputField.onValueChanged.RemoveAllListeners();
        }

        private void ToggleOptionsDropdown()
        {
            if (_optionsContents.activeSelf)
            {
                CloseOptionsDropdown();
            }
            else
            {
                OpenOptionsDropdown();
            }
        }

        private void OpenOptionsDropdown()
        {
            OnUiElementSelected(null);
            _optionsContents.SetActive(true);
        }

        private void CloseOptionsDropdown()
        {
            _optionsContents.SetActive(false);
        }

        private IEnumerator ShowSaveResetPopup(string message, Action<bool?> callback)
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

        private void OnResetButtonClicked()
        {
            StartCoroutine(ShowSaveResetPopup(ResetChangesText, resetChanges =>
            {
                if (resetChanges == null) return;
                if (resetChanges.Value == true) ResetChanges();
            }));
        }

        private void ResetChanges()
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
                && savedData.Orientation == compareData.Orientation;
        }

        private GameObject InstantiateBattleUiElement(BattleUiElementType uiElementType)
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
                BattleUiMovableElement movableElement = uiElementGameObject.GetComponent<BattleUiMovableElement>();
                editingComponent.SetInfo(movableElement);
            }

            // Setting listener for toggles
            _alignToGridToggle.onValueChanged.AddListener((value)=>
            {
                editingComponent.ToggleGrid(value);
                PlayerPrefs.SetInt(AlignToGridKey, value ? 1 : 0);
            });
            editingComponent.ToggleGrid(_alignToGridToggle.isOn);

            _incrementalScalingToggle.onValueChanged.AddListener((value) =>
            {
                editingComponent.ToggleIncrementScaling(value);
                PlayerPrefs.SetInt(IncrementalScalingKey, value ? 1 : 0);
            });
            editingComponent.ToggleIncrementScaling(_incrementalScalingToggle.isOn);

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

        private void SetDataToUiElement(BattleUiMovableElement movableElement)
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

            float handleSize = 0;

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
            size.x = Screen.width * (anchorMax.x - anchorMin.x);
            size.y = size.x / aspectRatio;

            Vector2 pos = new(
                (anchorMin.x + anchorMax.x) * 0.5f * Screen.width,
                (anchorMax.y + anchorMin.y) * 0.5f * Screen.height
            );

            (anchorMin, anchorMax) = CalculateAnchors(size, pos);

            return new(uiElementType, anchorMin, anchorMax, orientation, isFlippedHorizontally, isFlippedVertically, handleSize);
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

        private void OnUiElementSelected(BattleUiEditingComponent newSelectedEditingComponent)
        {
            CloseOptionsDropdown();
            _grid.RemoveLineHighlight();

            if (_currentlySelectedEditingComponent != null && _currentlySelectedEditingComponent != newSelectedEditingComponent)
            {
                _currentlySelectedEditingComponent.ShowControls(false);
            }

            _currentlySelectedEditingComponent = newSelectedEditingComponent;
        }

        private void UpdateInputFieldText(float value, TMP_InputField field)
        {
            bool isDecimal = field.contentType == TMP_InputField.ContentType.DecimalNumber;
            string valueString = isDecimal ? value.ToString("0.00") : value.ToString();
            field.SetTextWithoutNotify(valueString);
        }

        private void VerifyAndUpdateSliderValue(TMP_InputField field, Slider slider)
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

        private void UpdateGridColumnLines()
        {
            int columns = (int)_gridColumnsSlider.value;
            if (_grid.SetColumnLines(columns)) PlayerPrefs.SetInt(GridColumnLinesKey, columns);
        }

        private void UpdateGridRowLines()
        {
            int rows = (int)_gridRowsSlider.value;
            if (_grid.SetRowLines(rows)) PlayerPrefs.SetInt(GridRowLinesKey, rows);
        }

        private void UpdateArena()
        {
            float screenAspectRatio = Screen.width / (float)Screen.height;

            // Calculating arena scale.
            // If phone aspect ratio is same or thinner than the game aspect ratio we calculate arena width and height based on
            // editor width, but if it's thicker we calculate based on height so that the arena won't overlap or be too small.
            float arenaWidth;
            float arenaHeight; 
            if (screenAspectRatio <= GameAspectRatio)
            {
                arenaWidth = _arenaScaleSlider.value / 100f * EditorRect.width;
                arenaHeight = arenaWidth / GameAspectRatio;
            }
            else
            {
                arenaHeight = _arenaScaleSlider.value / 100f * EditorRect.height;
                arenaWidth = arenaHeight * GameAspectRatio;
            }
            
            // Calculating arena position
            Vector2 position = Vector2.zero;
            position.x = (_arenaPosXSlider.value / 100 * (EditorRect.width - arenaWidth)) + arenaWidth / 2f;
            position.y = ((100f - _arenaPosYSlider.value) / 100f * (EditorRect.height - arenaHeight)) + arenaHeight / 2f;

            // Calculating arena anchors
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.zero;

            anchorMin.x = (position.x - arenaWidth / 2.0f) / EditorRect.width;
            anchorMax.x = (position.x + arenaWidth / 2.0f) / EditorRect.width;

            anchorMin.y = (position.y - arenaHeight / 2.0f) / EditorRect.height;
            anchorMax.y = (position.y + arenaHeight / 2.0f) / EditorRect.height;

            // Setting arena anchors
            _arenaImage.anchorMin = anchorMin;
            _arenaImage.anchorMax = anchorMax;

            _arenaImage.offsetMin = Vector2.zero;
            _arenaImage.offsetMax = Vector2.zero;
        }

        private void NextInputCombination()
        {
            if ((int)_currentInputCombinationType < (int)InputCombinationType.Joysticks) _currentInputCombinationType++;
            else _currentInputCombinationType = 0;

            UpdateInputSettings();
        }

        private void PreviousInputCombination()
        {
            if ((int)_currentInputCombinationType > 0) _currentInputCombinationType--;
            else _currentInputCombinationType = InputCombinationType.Joysticks;

            UpdateInputSettings();
        }

        private void UpdateInputSettings()
        {
            // Initializing variables
            BattleMovementInputType movementType;
            BattleRotationInputType rotationType;

            string text;

            bool showSwipeMinDistance;
            bool showSwipeMaxDistance;
            bool showSwipeSensitivity;

            bool useGyroscope = _rotationGyroscopeOverrideToggle.isOn;

            // Setting values to variables based on current input combination
            switch (_currentInputCombinationType)
            {
                case InputCombinationType.SwipeTwoFinger:
                    movementType = BattleMovementInputType.Swipe;
                    rotationType = BattleRotationInputType.TwoFinger;

                    text = useGyroscope ? "Liiku pyyhk‰isem‰ll‰ &\nk‰‰nn‰ puhelinta k‰‰nt‰m‰ll‰" : "Liiku pyyhk‰isem‰ll‰ &\nk‰‰nn‰ kahdella sormella";

                    showSwipeMinDistance = true;
                    showSwipeMaxDistance = false;
                    showSwipeSensitivity = true;
                    break;

                case InputCombinationType.PointAndClickSwipe:
                    movementType = BattleMovementInputType.PointAndClick;
                    rotationType = BattleRotationInputType.Swipe;

                    text = useGyroscope ? "Liiku painamalla &\nk‰‰nn‰ puhelinta k‰‰nt‰m‰ll‰" : "Liiku painamalla &\nk‰‰nn‰ pyyhk‰isem‰ll‰";

                    showSwipeMinDistance = !useGyroscope;
                    showSwipeMaxDistance = !useGyroscope;
                    showSwipeSensitivity = false;
                    break;

                case InputCombinationType.Joysticks:
                    movementType = BattleMovementInputType.Joystick;
                    rotationType = BattleRotationInputType.Joystick;

                    text = useGyroscope ? "Liiku ohjausympyr‰ll‰ &\nk‰‰nn‰ puhelinta k‰‰nt‰m‰ll‰" : "Liiku & k‰‰nn‰ ohjausympyrˆill‰.";

                    showSwipeMinDistance = false;
                    showSwipeMaxDistance = false;
                    showSwipeSensitivity = false;

                    // Instantianting the joysticks if they are not yet instantiated
                    if (_instantiatedMoveJoystick == null)
                    {
                        _instantiatedMoveJoystick = InstantiateBattleUiElement(BattleUiElementType.MoveJoystick).GetComponent<BattleUiMovableElement>();
                        SetDataToUiElement(_instantiatedMoveJoystick);
                    }

                    if (_instantiatedRotateJoystick == null)
                    {
                        _instantiatedRotateJoystick = InstantiateBattleUiElement(BattleUiElementType.RotateJoystick).GetComponent<BattleUiMovableElement>();
                        SetDataToUiElement(_instantiatedRotateJoystick);
                    }
                    break;

                default:
                    movementType = SettingsCarrier.BattleMovementInputDefault;
                    rotationType = SettingsCarrier.BattleRotationInputDefault;

                    text = "Virhe, t‰t‰ vaihtoehtoa ei lˆydy";

                    showSwipeMinDistance = false;
                    showSwipeMaxDistance = false;
                    showSwipeSensitivity = false;
                    break;
            }

            // Overriding rotation with gyroscope if it's enabled
            if (useGyroscope) rotationType = BattleRotationInputType.Gyroscope;

            // Setting input values to settings carrier
            SettingsCarrier.Instance.BattleMovementInput = movementType;
            SettingsCarrier.Instance.BattleRotationInput = rotationType;

            // Setting text to the input selector label
            _inputSelectorLabel.text = text;

            // Setting visibility for the swipe and gyroscope additional options
            _swipeMinDistanceHolder.SetActive(showSwipeMinDistance);
            _swipeMaxDistanceHolder.SetActive(showSwipeMaxDistance);
            _movementSwipeSensitivityHolder.SetActive(showSwipeSensitivity);
            _gyroscopeMinAngleHolder.SetActive(useGyroscope);

            // Setting visibility to joysticks
            if (_instantiatedMoveJoystick != null) _instantiatedMoveJoystick.gameObject.SetActive(movementType == BattleMovementInputType.Joystick);
            if (_instantiatedRotateJoystick != null) _instantiatedRotateJoystick.gameObject.SetActive(rotationType == BattleRotationInputType.Joystick);
        }
    }
}
