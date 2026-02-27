/// @file BattleUiMovableJoystickElement.cs
/// <summary>
/// Contains @cref{Altzone.Scripts.BattleUiShared,BattleUiMovableJoystickElement} which sets the %Battle Ui movable joystick element's data.
/// </summary>
///
/// This script:<br/>
/// Handles setting the %Battle Ui joystick element's size and locked status.

using UnityEngine;
using UnityEngine.UI;

using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// <span class="brief-h">Movable joystick BattleUiMovableElement.</span><br/>
    /// Handles setting the %Battle Ui movable joystick element's size and locked status.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMovableJoystickElement : BattleUiMovableElement
    {
        /// @anchor BattleUiMovableJoystickElement-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to joystick background's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Image.html">Image@u-exlink</a> component.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [Header("Component references")]
        [SerializeField] private Image _backgroundImage;

        /// <summary>[SerializeField] Reference to joystick background's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [SerializeField] private RectTransform _backgroundRectTransform;

        /// <summary>[SerializeField] Reference to joystick handle's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Image.html">Image@u-exlink</a> component.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [SerializeField] private Image _handleImage;

        /// <summary>[SerializeField] Reference to joystick handle's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [SerializeField] private RectTransform _handleRectTransform;

        /// <summary>[SerializeField] Movement joystick's handle icon <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Sprite.html">Sprite@u-exlink</a>.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [Header("Sprite references")]
        [SerializeField] private Sprite _moveIcon;

        /// <summary>[SerializeField] Movement joystick's background <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Sprite.html">Sprite@u-exlink</a>.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [SerializeField] private Sprite _moveBackground;

        /// <summary>[SerializeField] Rotation joystick's handle icon <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Sprite.html">Sprite@u-exlink</a>.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [SerializeField] private Sprite _rotateIcon;

        /// <summary>[SerializeField] Rotation joystick's background <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Sprite.html">Sprite@u-exlink</a>.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [SerializeField] private Sprite _rotateBackground;

        /// <summary>[SerializeField] Locked joystick's handle icon <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Sprite.html">Sprite@u-exlink</a>.</summary>
        /// @ref BattleUiMovableJoystickElement-SerializeFields
        [SerializeField] private Sprite _lockIcon;

        /// @}

        /// <value>Minimum size for %Battle Ui joystick's handle.</value>
        public const int HandleSizeMin = 50;

        /// <value>Maximum size for %Battle Ui joystick's handle.</value>
        public const int HandleSizeMax = 250;

        /// <value>Default size for %Battle Ui joystick's handle.</value>
        public const int HandleSizeDefault = 150;

        /// <summary>
        /// Set BattleUiMovableElementData to this BattleUiMovableJoystickElement.
        /// </summary>
        ///
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
                    _backgroundImage.pixelsPerUnitMultiplier = Mathf.Pow((handleSize - HandleSizeMin) / (HandleSizeMax - HandleSizeMin), 0.35f) * (HandleSizeMaxPPU - HandleSizeMinPPU) + HandleSizeMinPPU;

                    // Workaround to set height according to handle height since the rotate joystick has to be same height as the handle
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _handleRectTransform.rect.height);
                    break;
            }
        }

        /// <summary>
        /// Get the data from this Ui joystick element.
        /// </summary>
        ///
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

        /// <summary>
        /// Set this %Battle Ui joystick element locked status.
        /// </summary>
        ///
        /// <param name="locked">If %Battle Ui joystick element should be locked or not.</param>
        public void SetLocked(bool locked)
        {
            _backgroundImage.raycastTarget = !locked;

            if (locked) _handleImage.sprite = _lockIcon;
            else _handleImage.sprite = UiElementType == BattleUiElementType.MoveJoystick ? _moveIcon : _rotateIcon;
        }

        /// <value>Minimum value for #_handleImage's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Image.html#UnityEngine_UI_Image_pixelsPerUnitMultiplier">pixelsPerUnitMultiplier@u-exlink</a>.</value>
        private const float HandleSizeMinPPU = 0.5f;

        /// <value>Maximum value for #_handleImage's <a href="https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Image.html#UnityEngine_UI_Image_pixelsPerUnitMultiplier">pixelsPerUnitMultiplier@u-exlink</a>.</value>
        private const float HandleSizeMaxPPU = 0.125f;
    }
}
