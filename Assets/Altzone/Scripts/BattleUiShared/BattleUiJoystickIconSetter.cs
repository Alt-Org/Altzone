using UnityEngine;
using UnityEngine.UI;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Handles setting the joystick icon.
    /// </summary>
    public class BattleUiJoystickIconSetter : MonoBehaviour
    {
        [Header("Icon sprites")]
        [SerializeField] private Sprite _moveIcon;
        [SerializeField] private Sprite _rotateIcon;

        [Header("Component reference")]
        [SerializeField] private Image _iconImage;

        /// <summary>
        /// Set icon to the joystick.
        /// </summary>
        /// <param name="uiElementType">The joystick's BattleUiElementType to differentiate between the rotate and move joysticks.</param>
        public void SetIcon(BattleUiElementType uiElementType)
        {
            switch (uiElementType)
            {
                case BattleUiElementType.MoveJoystick:
                    _iconImage.sprite = _moveIcon;
                    break;
                case BattleUiElementType.RotateJoystick:
                    _iconImage.sprite = _rotateIcon;
                    break;
            }
        }
    }
}
