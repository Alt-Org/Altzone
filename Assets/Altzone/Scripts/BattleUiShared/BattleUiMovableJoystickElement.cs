using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Handles setting and getting the Battle Ui movable joystick element position.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiMovableJoystickElement : BattleUiMovableElement
    {
        [SerializeField] private Image _handleImage;
        [SerializeField] private RectTransform _handleRectTransform;
    }
}
