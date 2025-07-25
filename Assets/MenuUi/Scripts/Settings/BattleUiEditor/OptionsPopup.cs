using UnityEngine;
using UnityEngine.UI;

using TMPro;
using static MenuUi.Scripts.Settings.BattleUiEditor.BattleUiEditor;
using Altzone.Scripts.BattleUiShared;
using static SettingsCarrier;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    public class OptionsPopup : MonoBehaviour
    {
        [ Header("Options popup")]
        [SerializeField] private GameObject _optionsContents;
        [SerializeField] private Button _resetButton;

        [Header("Input options")]
        [SerializeField] public Toggle _swipeMovementToggle;
        [SerializeField] public Toggle _pointAndClickMovementToggle;
        [SerializeField] public Toggle _joystickMovementToggle;
        [Space]
        [SerializeField] private Toggle _twoFingerRotationToggle;
        [SerializeField] private Toggle _swipeRotationToggle;
        [SerializeField] private Toggle _joystickRotationToggle;
        [SerializeField] private Toggle _gyroscopeRotationToggle;
        [Space]
        [SerializeField] private GameObject _swipeMinDistanceHolder;
        [SerializeField] public Slider _swipeMinDistanceSlider;
        [SerializeField] public TMP_InputField _swipeMinDistanceInputField;
        [Space]
        [SerializeField] private GameObject _swipeMaxDistanceHolder;
        [SerializeField] public Slider _rotationSwipeMaxDistanceSlider;
        [SerializeField] public TMP_InputField _rotationSwipeMaxDistanceInputField;
        [Space]
        [SerializeField] private GameObject _movementSwipeSensitivityHolder;
        [SerializeField] public Slider _movementSwipeSensitivitySlider;
        [SerializeField] public TMP_InputField _movementSwipeSensitivityInputField;
        [Space]
        [SerializeField] private GameObject _gyroscopeMinAngleHolder;
        [SerializeField] public Slider _gyroscopeMinAngleSlider;
        [SerializeField] public TMP_InputField _gyroscopeMinAngleInputField;

        [Header("Arena options")]
        [SerializeField] public Slider _arenaScaleSlider;
        [SerializeField] private TMP_InputField _arenaScaleInputField;
        [Space]
        [SerializeField] public Slider _arenaPosXSlider;
        [SerializeField] private TMP_InputField _arenaPosXInputField;
        [Space]
        [SerializeField] public Slider _arenaPosYSlider;
        [SerializeField] private TMP_InputField _arenaPosYInputField;

        private const string ResetChangesText = "Palauta UI-elementtien oletusasettelu?";
        private BattleUiEditor _battleUiEditor;
        private const float GameAspectRatio = 9f / 16f;

        private BattleUiMovableElement _instantiatedMoveJoystick;
        private BattleUiMovableElement _instantiatedRotateJoystick;

        private void Awake()
        {
            _resetButton.onClick.AddListener(OnResetButtonClicked);
            CloseOptionsPopup();

            // Input options listeners
            _swipeMovementToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(BattleMovementInputType.Swipe, BattleRotationInputType.TwoFinger); });
            _pointAndClickMovementToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(BattleMovementInputType.PointAndClick, BattleRotationInputType.TwoFinger); });
            _joystickMovementToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(BattleMovementInputType.Joystick, BattleRotationInputType.Joystick); });

            _twoFingerRotationToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, BattleRotationInputType.TwoFinger); });
            _swipeRotationToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, BattleRotationInputType.Swipe); });
            _joystickRotationToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, BattleRotationInputType.Joystick); });
            _gyroscopeRotationToggle.onValueChanged.AddListener((value) => { if (value) UpdateInputSettings(SettingsCarrier.Instance.BattleMovementInput, BattleRotationInputType.Gyroscope); });

            _swipeMinDistanceSlider.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.UpdateInputFieldText(value, _swipeMinDistanceInputField);
                SettingsCarrier.Instance.BattleSwipeMinDistance = value;
            });
            _swipeMinDistanceInputField.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.VerifyAndUpdateSliderValue(_swipeMinDistanceInputField, _swipeMinDistanceSlider);
                SettingsCarrier.Instance.BattleSwipeMinDistance = _swipeMinDistanceSlider.value;
            });

            _rotationSwipeMaxDistanceSlider.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.UpdateInputFieldText(value, _rotationSwipeMaxDistanceInputField);
                SettingsCarrier.Instance.BattleSwipeMaxDistance = value;
            });
            _rotationSwipeMaxDistanceInputField.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.VerifyAndUpdateSliderValue(_rotationSwipeMaxDistanceInputField, _rotationSwipeMaxDistanceSlider);
                SettingsCarrier.Instance.BattleSwipeMaxDistance = _rotationSwipeMaxDistanceSlider.value;
            });

            _movementSwipeSensitivitySlider.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.UpdateInputFieldText(value, _movementSwipeSensitivityInputField);
                SettingsCarrier.Instance.BattleSwipeSensitivity = value;
            });
            _movementSwipeSensitivityInputField.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.VerifyAndUpdateSliderValue(_movementSwipeSensitivityInputField, _movementSwipeSensitivitySlider);
                SettingsCarrier.Instance.BattleSwipeSensitivity = _movementSwipeSensitivitySlider.value;
            });

            _gyroscopeMinAngleSlider.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.UpdateInputFieldText(value, _gyroscopeMinAngleInputField);
                SettingsCarrier.Instance.BattleGyroMinAngle = value;
            });
            _gyroscopeMinAngleInputField.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.VerifyAndUpdateSliderValue(_gyroscopeMinAngleInputField, _gyroscopeMinAngleSlider);
                SettingsCarrier.Instance.BattleGyroMinAngle = _gyroscopeMinAngleSlider.value;
            });

            // Arena scale listeners
            _arenaScaleSlider.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.UpdateInputFieldText(value, _arenaScaleInputField);
                SettingsCarrier.Instance.BattleArenaScale = (int)value;
                UpdateArena();
            });
            _arenaScaleInputField.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.VerifyAndUpdateSliderValue(_arenaScaleInputField, _arenaScaleSlider);
                SettingsCarrier.Instance.BattleArenaScale = (int)_arenaScaleSlider.value;
                UpdateArena();
            });

            // Arena pos x listeners
            _arenaPosXSlider.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.UpdateInputFieldText(value, _arenaPosXInputField);
                SettingsCarrier.Instance.BattleArenaPosX = (int)value;
                UpdateArena();
            });
            _arenaPosXInputField.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.VerifyAndUpdateSliderValue(_arenaPosXInputField, _arenaPosXSlider);
                SettingsCarrier.Instance.BattleArenaPosX = (int)_arenaPosXSlider.value;
                UpdateArena();
            });

            // Arena pos y listeners
            _arenaPosYSlider.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.UpdateInputFieldText(value, _arenaPosYInputField);
                SettingsCarrier.Instance.BattleArenaPosY = (int)value;
                UpdateArena();
            });
            _arenaPosYInputField.onValueChanged.AddListener((value) =>
            {
                _battleUiEditor.VerifyAndUpdateSliderValue(_arenaPosYInputField, _arenaPosYSlider);
                SettingsCarrier.Instance.BattleArenaPosY = (int)_arenaPosYSlider.value;
                UpdateArena();
            });
        }

        private void OnDestroy()
        {
            _resetButton.onClick.RemoveAllListeners();

            // Removing input options listeners
            _swipeMovementToggle.onValueChanged.RemoveAllListeners();
            _pointAndClickMovementToggle.onValueChanged.RemoveAllListeners();
            _joystickMovementToggle.onValueChanged.RemoveAllListeners();

            _twoFingerRotationToggle.onValueChanged.RemoveAllListeners();
            _swipeRotationToggle.onValueChanged.RemoveAllListeners();
            _joystickRotationToggle.onValueChanged.RemoveAllListeners();
            _gyroscopeRotationToggle.onValueChanged.RemoveAllListeners();

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

        public void Initialize(BattleUiEditor battleUiEditor)
        {
            _battleUiEditor = battleUiEditor;
        }

        public void UpdateInputSettings(BattleMovementInputType movementType, BattleRotationInputType rotationType)
        {
            // Setting input values to settings carrier
            SettingsCarrier.Instance.BattleMovementInput = movementType;
            SettingsCarrier.Instance.BattleRotationInput = rotationType;

            // If joystick movement was selected instantianting the joysticks if they are not yet instantiated
            if (movementType == BattleMovementInputType.Joystick)
            {
                if (_instantiatedMoveJoystick == null)
                {
                    _instantiatedMoveJoystick = _battleUiEditor.InstantiateBattleUiElement(BattleUiElementType.MoveJoystick).GetComponent<BattleUiMovableElement>();
                    _battleUiEditor.SetDataToUiElement(_instantiatedMoveJoystick);
                }

                if (_instantiatedRotateJoystick == null)
                {
                    _instantiatedRotateJoystick = _battleUiEditor.InstantiateBattleUiElement(BattleUiElementType.RotateJoystick).GetComponent<BattleUiMovableElement>();
                    _battleUiEditor.SetDataToUiElement(_instantiatedRotateJoystick);
                }
            }

            // Toggling rotation toggles isOn based on rotation type and visibility based on movement type
            _twoFingerRotationToggle.SetIsOnWithoutNotify(rotationType == BattleRotationInputType.TwoFinger);
            _twoFingerRotationToggle.gameObject.SetActive(movementType == BattleMovementInputType.Swipe || movementType == BattleMovementInputType.PointAndClick);

            _swipeRotationToggle.SetIsOnWithoutNotify(rotationType == BattleRotationInputType.Swipe);
            _swipeRotationToggle.gameObject.SetActive(movementType == BattleMovementInputType.PointAndClick);

            _joystickRotationToggle.SetIsOnWithoutNotify(rotationType == BattleRotationInputType.Joystick);
            _joystickRotationToggle.gameObject.SetActive(movementType == BattleMovementInputType.Joystick);

            _gyroscopeRotationToggle.SetIsOnWithoutNotify(rotationType == BattleRotationInputType.Gyroscope);

            // Setting visibility for the swipe and gyroscope additional options
            _swipeMinDistanceHolder.SetActive(movementType == BattleMovementInputType.Swipe || rotationType == BattleRotationInputType.Swipe);
            _swipeMaxDistanceHolder.SetActive(rotationType == BattleRotationInputType.Swipe);
            _movementSwipeSensitivityHolder.SetActive(movementType == BattleMovementInputType.Swipe);
            _gyroscopeMinAngleHolder.SetActive(rotationType == BattleRotationInputType.Gyroscope);

            // Setting visibility to joysticks
            if (_instantiatedMoveJoystick != null) _instantiatedMoveJoystick.gameObject.SetActive(movementType == BattleMovementInputType.Joystick);
            if (_instantiatedRotateJoystick != null) _instantiatedRotateJoystick.gameObject.SetActive(rotationType == BattleRotationInputType.Joystick);
        }

        private void UpdateArena()
        {
            float screenAspectRatio = Screen.width / (float)Screen.height;

            // For some reason the editor has different aspect ratio calculated from rect size in local space than in world space because of the editor scaling
            // Getting editor corners in world space
            Vector3[] editorCorners = new Vector3[4];
            EditorRectTransform.GetWorldCorners(editorCorners);

            // Calculating world space size and aspect ratio
            Vector2 editorWorldSize = new(editorCorners[(int)CornerType.TopRight].x - editorCorners[(int)CornerType.TopLeft].x,
                       editorCorners[(int)CornerType.TopRight].y - editorCorners[(int)CornerType.BottomRight].y);
            float editorWorldAspectRatio = editorWorldSize.x / editorWorldSize.y;

            // Calculating a height for the editor from the world aspect ratio so that it works in calculations
            float editorAspectRatioHeight = EditorRect.width / editorWorldAspectRatio;

            // Calculating arena scale.
            // If phone aspect ratio is same or thinner than the game aspect ratio we calculate arena width and height based on
            // editor width, but if it's thicker we calculate based on height so that the arena won't overlap or be too small.
            float arenaWidth;
            float arenaHeight;
            if (screenAspectRatio <= GameAspectRatio)
            {
                arenaWidth = _arenaScaleSlider.value * 0.01f * EditorRect.width;
                arenaHeight = arenaWidth / GameAspectRatio;
            }
            else
            {
                arenaHeight = _arenaScaleSlider.value * 0.01f * editorAspectRatioHeight;
                arenaWidth = arenaHeight * GameAspectRatio;
            }

            // Calculating arena position
            Vector2 position = Vector2.zero;
            position.x += _arenaPosXSlider.value * 0.01f * (EditorRect.width - arenaWidth);
            position.y += (100f - _arenaPosYSlider.value) * 0.01f * (editorAspectRatioHeight - arenaHeight);

            // Calculating arena anchors
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.zero;

            anchorMin.x = position.x / EditorRect.width;
            anchorMax.x = (position.x + arenaWidth) / EditorRect.width;

            anchorMin.y = position.y / editorAspectRatioHeight;
            anchorMax.y = (position.y + arenaHeight) / editorAspectRatioHeight;

            // Setting arena anchors
            _battleUiEditor._arenaImage.anchorMin = anchorMin;
            _battleUiEditor._arenaImage.anchorMax = anchorMax;

            _battleUiEditor._arenaImage.offsetMin = Vector2.zero;
            _battleUiEditor._arenaImage.offsetMax = Vector2.zero;
        }

        public void ToggleOptionsPopup()
        {
            if (_optionsContents.activeSelf)
            {
                CloseOptionsPopup();
            }
            else
            {
                OpenOptionsPopup();
            }
        }

        public void OpenOptionsPopup()
        {
            _battleUiEditor.OnUiElementSelected(null);
            _optionsContents.SetActive(true);
        }

        public void CloseOptionsPopup()
        {
            _optionsContents.SetActive(false);
        }

        private void OnResetButtonClicked()
        {
            _battleUiEditor.StartCoroutine(_battleUiEditor.ShowSaveResetPopup(ResetChangesText, resetChanges =>
            {
                if (resetChanges == null) return;
                if (resetChanges.Value == true) _battleUiEditor.ResetChanges();
            }));
        }
    }
}
