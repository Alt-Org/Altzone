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
                    _backgroundImage.pixelsPerUnitMultiplier = 0.2f;

                    // Workaround to set height according to handle height since the rotate joystick has to be same height as the handle
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _handleRectTransform.rect.height);
                    break;
            }
        }

        public void SetLocked(bool locked)
        {
            _backgroundImage.raycastTarget = !locked;

            if (locked) _handleImage.sprite = _lockIcon;
            else _handleImage.sprite = UiElementType == BattleUiElementType.MoveJoystick ? _moveIcon : _rotateIcon;
        }
        
        private const int HandleSizeMin = 50;
        private const int HandleSizeMax = 250;
        private const int HandleSizeDefault = 150;
    }
}
