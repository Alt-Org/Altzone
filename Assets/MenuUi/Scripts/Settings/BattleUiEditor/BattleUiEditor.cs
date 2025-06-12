using System;
using System.Collections;
using System.Collections.Generic;

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
        [SerializeField] private RectTransform _arenaImage;
        [SerializeField] private RectTransform _uiElementsHolder;
        [SerializeField] private GridController _grid;

        [Header("Options dropdown references")]
        [SerializeField] private Button _optionsDropdownButton;
        [SerializeField] private Image _optionsDropdownButtonImage;
        [SerializeField] private GameObject _optionsDropdownContents;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Toggle _incrementalScalingToggle;

        [Header("Arena options")]
        [SerializeField] private Slider _arenaScaleSlider;
        [SerializeField] private TMP_InputField _arenaScaleInputField;
        [SerializeField] private Slider _arenaPosXSlider;
        [SerializeField] private TMP_InputField _arenaPosXInputField;
        [SerializeField] private Slider _arenaPosYSlider;
        [SerializeField] private TMP_InputField _arenaPosYInputField;

        [Header("Grid options")]
        [SerializeField] private Toggle _showGridToggle;
        [SerializeField] private Toggle _alignToGridToggle;
        [SerializeField] private Slider _gridColumnsSlider;
        [SerializeField] private TMP_InputField _gridColumnsInputField;
        [SerializeField] private Slider _gridRowsSlider;
        [SerializeField] private TMP_InputField _gridRowsInputField;

        [Header("BattleUi prefabs")]
        [SerializeField] private GameObject _editingComponent;
        [SerializeField] private GameObject _timer;
        [SerializeField] private GameObject _playerInfo;
        [SerializeField] private GameObject _diamonds;
        [SerializeField] private GameObject _giveUpButton;
        [SerializeField] private GameObject _joystick;

        [Header("Save/reset popup")]
        [SerializeField] private GameObject _saveResetPopup;
        [SerializeField] private TMP_Text _popupText;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _noButton;
        
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

            if (_instantiatedPlayerInfo == null)
            {
                _instantiatedPlayerInfo = InstantiateBattleUiElement(BattleUiElementType.PlayerInfo).GetComponent<BattleUiMultiOrientationElement>();

                TextMeshProUGUI playerNameHorizontal = _instantiatedPlayerInfo.HorizontalConfiguration.GetComponentInChildren<TextMeshProUGUI>();
                TextMeshProUGUI playerNameVertical = _instantiatedPlayerInfo.VerticalConfiguration.GetComponentInChildren<TextMeshProUGUI>();
                
                if (playerNameHorizontal != null) playerNameHorizontal.text = PlayerText;
                if (playerNameVertical != null) playerNameVertical.text = PlayerText;
            }

            if (_instantiatedTeammateInfo == null)
            {
                _instantiatedTeammateInfo = InstantiateBattleUiElement(BattleUiElementType.TeammateInfo).GetComponent<BattleUiMultiOrientationElement>();

                TextMeshProUGUI teammateNameHorizontal = _instantiatedTeammateInfo.HorizontalConfiguration.GetComponentInChildren<TextMeshProUGUI>();
                TextMeshProUGUI teammateNameVertical = _instantiatedTeammateInfo.VerticalConfiguration.GetComponentInChildren<TextMeshProUGUI>();
                if (teammateNameHorizontal != null) teammateNameHorizontal.text = TeammateText;
                if (teammateNameVertical != null) teammateNameVertical.text = TeammateText;
            }

            if (_instantiatedMoveJoystick == null) _instantiatedMoveJoystick = InstantiateBattleUiElement(BattleUiElementType.MoveJoystick).GetComponent<BattleUiMovableElement>();
            if (_instantiatedRotateJoystick == null) _instantiatedRotateJoystick = InstantiateBattleUiElement(BattleUiElementType.RotateJoystick).GetComponent<BattleUiMovableElement>();

            // Setting data to Ui elements
            SetDataToUiElement(_instantiatedTimer);
            SetDataToUiElement(_instantiatedDiamonds);
            SetDataToUiElement(_instantiatedGiveUpButton);
            SetDataToUiElement(_instantiatedPlayerInfo);
            SetDataToUiElement(_instantiatedTeammateInfo);
            SetDataToUiElement(_instantiatedMoveJoystick);
            SetDataToUiElement(_instantiatedRotateJoystick);
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

        private const string PlayerText = "Minä";
        private const string TeammateText = "Tiimikaveri";

        private const string SaveChangesText = "Tallenna muutokset?";
        private const string ResetChangesText = "Palauta UI-elementtien oletusasettelu?";

        private const string GridColumnLinesKey = "BattleUiEditorGridColumns";
        private const string GridRowLinesKey = "BattleUiEditorGridRows";
        private const string ShowGridKey = "BattleUiEditorShowGrid";
        private const string AlignToGridKey = "BattleUiEditorAlignToGrid";
        private const string IncrementalScalingKey = "BattleUiEditorIncScaling";

        private const int GridRowLinesDefault = 39;
        private const int GridColumnLinesDefault = 19;

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

        private void Awake()
        {
            EditorRectTransform = GetComponent<RectTransform>();
            EditorRect = EditorRectTransform.rect;

            // Close and save button listeners
            _closeButton.onClick.AddListener(CloseEditor);
            _saveButton.onClick.AddListener(SaveChanges);

            // Options dropdown listeners
            _optionsDropdownButton.onClick.AddListener(ToggleOptionsDropdown);

            _resetButton.onClick.AddListener(OnResetButtonClicked);

            // Arena scale listeners
            _arenaScaleSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _arenaScaleInputField);
                SettingsCarrier.Instance.BattleArenaScale = (int)value;
                UpdateArena();
            });
            _arenaPosXSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _arenaPosXInputField);
                SettingsCarrier.Instance.BattleArenaPosX = (int)value;
                UpdateArena();
            });
            _arenaPosYSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _arenaPosYInputField);
                SettingsCarrier.Instance.BattleArenaPosY = (int)value;
                UpdateArena();
            });

            _arenaScaleInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_arenaScaleInputField, _arenaScaleSlider);
                SettingsCarrier.Instance.BattleArenaScale = (int)_arenaScaleSlider.value;
                UpdateArena();
            });
            _arenaPosXInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_arenaPosXInputField, _arenaPosXSlider);
                SettingsCarrier.Instance.BattleArenaPosX = (int)_arenaPosXSlider.value;
                UpdateArena();
            });
            _arenaPosYInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_arenaPosYInputField, _arenaPosYSlider);
                SettingsCarrier.Instance.BattleArenaPosY = (int)_arenaPosYSlider.value;
                UpdateArena();
            });

            // Grid listeners
            _gridColumnsSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _gridColumnsInputField);
                UpdateGridColumnLines();
            });

            _gridRowsSlider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFieldText(value, _gridRowsInputField);
                UpdateGridRowLines();
            });

            _gridColumnsInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_gridColumnsInputField, _gridColumnsSlider);
                UpdateGridColumnLines();
            });

            _gridRowsInputField.onValueChanged.AddListener((value) =>
            {
                VerifyAndUpdateSliderValue(_gridRowsInputField, _gridRowsSlider);
                UpdateGridRowLines();
            });

            _showGridToggle.onValueChanged.AddListener((value)=>
            {
                _grid.SetShow(value);
                PlayerPrefs.SetInt(ShowGridKey, value ? 1 : 0);
            });
        }

        private void Start()
        {
            // Getting saved settings. This will invoke the listeners added in Awake so the input fields will be updated as well.
            _arenaScaleSlider.value = SettingsCarrier.Instance.BattleArenaScale;
            _arenaPosXSlider.value = SettingsCarrier.Instance.BattleArenaPosX;
            _arenaPosYSlider.value = SettingsCarrier.Instance.BattleArenaPosY;

            // Grid and incremental scaling settings are saved locally from this script because they aren't accessed anywhere else
            _gridColumnsSlider.value = PlayerPrefs.GetInt(GridColumnLinesKey, GridColumnLinesDefault);
            _gridRowsSlider.value = PlayerPrefs.GetInt(GridRowLinesKey, GridRowLinesDefault);
            _showGridToggle.isOn = PlayerPrefs.GetInt(ShowGridKey, 1) == 1;
            _alignToGridToggle.isOn = PlayerPrefs.GetInt(AlignToGridKey, 1) == 1;

            _incrementalScalingToggle.isOn = PlayerPrefs.GetInt(IncrementalScalingKey, 1) == 1;

            // Initializing grid
            _grid.SetRowLines((int)_gridRowsSlider.value);
            _grid.SetColumnLines((int)_gridColumnsSlider.value);
            _grid.SetShow(_showGridToggle.isOn);
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
            _optionsDropdownButton.onClick.RemoveAllListeners();

            _resetButton.onClick.RemoveAllListeners();
            _alignToGridToggle.onValueChanged.RemoveAllListeners();
            _incrementalScalingToggle.onValueChanged.RemoveAllListeners();

            // Removing arena scale listeners
            _arenaScaleSlider.onValueChanged.RemoveAllListeners();
            _arenaPosXSlider.onValueChanged.RemoveAllListeners();
            _arenaPosYSlider.onValueChanged.RemoveAllListeners();

            // Removing grid listeners
            _gridColumnsSlider.onValueChanged.RemoveAllListeners();
            _gridRowsSlider.onValueChanged.RemoveAllListeners();

            _showGridToggle.onValueChanged.RemoveAllListeners();
        }

        private void ToggleOptionsDropdown()
        {
            if (_optionsDropdownContents.activeSelf)
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
            _optionsDropdownContents.SetActive(true);
        }

        private void CloseOptionsDropdown()
        {
            _optionsDropdownContents.SetActive(false);
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

            BattleUiMovableElementData moveJoystickData = _instantiatedMoveJoystick.GetData();
            if (!IsSavedDataSimilar(BattleUiElementType.MoveJoystick)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.MoveJoystick, moveJoystickData);

            BattleUiMovableElementData rotateJoystickData = _instantiatedRotateJoystick.GetData();
            if (!IsSavedDataSimilar(BattleUiElementType.RotateJoystick)) SettingsCarrier.Instance.SetBattleUiMovableElementData(BattleUiElementType.RotateJoystick, rotateJoystickData);

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
                    compareData = _instantiatedMoveJoystick.GetData();
                    break;
                case BattleUiElementType.RotateJoystick:
                    compareData = _instantiatedRotateJoystick.GetData();
                    break;
                default: // Checking if saved data is similar for every ui element
                    // Note: if more ui elements are added change from GiveUpButton to the last element in the enum
                    bool isSavedDataSimilar = true;
                    for (int i = 0; i <= (int)BattleUiElementType.GiveUpButton; i++) 
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

                    anchorMin.y = 0.025f;
                    anchorMax.y = 0.075f;
                    movableUiElementRect = _instantiatedDiamonds.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.GiveUpButton:
                    if (_instantiatedGiveUpButton == null) return null;

                    anchorMin.x = 0.05f;
                    anchorMax.x = 0.25f;

                    anchorMin.y = 0.025f;
                    anchorMax.y = 0.075f;

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

                    anchorMin.x = 0f;
                    anchorMax.x = 0.4f;

                    anchorMin.y = 0f;
                    anchorMax.y = 0.2f;

                    movableUiElementRect = _instantiatedMoveJoystick.GetComponent<RectTransform>().rect;
                    break;

                case BattleUiElementType.RotateJoystick:
                    if (_instantiatedRotateJoystick == null) return null;

                    anchorMin.x = 0.6f;
                    anchorMax.x = 1f;

                    anchorMin.y = 0f;
                    anchorMax.y = 0.2f;

                    movableUiElementRect = _instantiatedRotateJoystick.GetComponent<RectTransform>().rect;
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

            return new(anchorMin, anchorMax, orientation, isFlippedHorizontally, isFlippedVertically);
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
            field.SetTextWithoutNotify(value.ToString());
        }

        private void VerifyAndUpdateSliderValue(TMP_InputField field, Slider slider)
        {
            if (int.TryParse(field.text, out int value))
            {
                int clampedValue = Math.Clamp(value, (int)slider.minValue, (int)slider.maxValue);
                field.SetTextWithoutNotify(clampedValue.ToString());
                slider.SetValueWithoutNotify(clampedValue);
            }
            else
            {
                field.SetTextWithoutNotify(slider.minValue.ToString());
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
    }
}
