using UnityEngine;
using UnityEngine.UI;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Handles setting and getting the Battle Ui movable joystick element position.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMovableJoystickElement : BattleUiMovableElement
    {
        [Header("Component references")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private RectTransform _backgroundRectTransform;
        [SerializeField] private Image _handleImage;
        [SerializeField] private RectTransform _handleRectTransform;
        [Header("Sprite references")]
        [SerializeField] private Sprite _moveIcon;
        [SerializeField] private Sprite _moveBackground;
        [SerializeField] private Sprite _rotateIcon;
        [SerializeField] private Sprite _rotateBackground;
        [SerializeField] private Sprite _lockIcon;

        public const int HandleSizeMin = 50;
        public const int HandleSizeMax = 250;
        public const int HandleSizeDefault = 150;

        /// <summary>
        /// Set BattleUiMovableElementData to this BattleUiMovableJoystickElement.
        /// </summary>
        /// <param name="data">The data which to set to this Ui element.</param>
        public override void SetData(BattleUiMovableElementData data)
        {
            base.SetData(data);

            // Setting handle size
            float handleSize = data.HandleSize == 0 ? HandleSizeDefault : Mathf.Clamp(data.HandleSize, HandleSizeMin, Mathf.Max(HandleSizeMax, _rectTransform.rect.width));
            _handleRectTransform.sizeDelta = new Vector2(handleSize, handleSize);

            // Setting handle icon and background images and the background anchors
            switch (data.UiElementType)
            {
                case BattleUiElementType.MoveJoystick:
                    _handleImage.sprite = _moveIcon;
                    _backgroundImage.sprite = _moveBackground;
                    break;

                case BattleUiElementType.RotateJoystick:
                    _handleImage.sprite = _rotateIcon;
                    _backgroundImage.sprite = _rotateBackground;
                    _backgroundImage.type = Image.Type.Sliced;
                    _backgroundImage.pixelsPerUnitMultiplier = (handleSize - HandleSizeMin) / (HandleSizeMax - HandleSizeMin) * (HandleSizeMaxPPU - HandleSizeMinPPU) + HandleSizeMinPPU;

                    // Workaround to set height according to handle height since the rotate joystick has to be same height as the handle
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _handleRectTransform.rect.height);
                    break;
            }
        }

        /// <summary>
        /// Get the data from this Ui joystick element.
        /// </summary>
        /// <returns>Returns BattleUiMovableElementData serializable object. Null if couldn't get valid data.</returns>
        public override BattleUiMovableElementData GetData()
        {
            if (_rectTransform != null && _handleRectTransform != null)
            {
                return new BattleUiMovableElementData(
                    uiElementType: UiElementType,
                    transparency: _currentTransparency,
                    anchorMin: _rectTransform.anchorMin,
                    anchorMax: _rectTransform.anchorMax,
                    handleSize: (int)_handleRectTransform.rect.width
                );
            }
            else
            {
                return null;
            }
        }

        public void SetLocked(bool locked)
        {
            _backgroundImage.raycastTarget = !locked;

            if (locked) _handleImage.sprite = _lockIcon;
            else _handleImage.sprite = UiElementType == BattleUiElementType.MoveJoystick ? _moveIcon : _rotateIcon;
        }

        private const float HandleSizeMinPPU = 0.5f;
        private const float HandleSizeMaxPPU = 0.125f;
    }
}
