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
        [Header("Component references")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _handleImage;
        [SerializeField] private RectTransform _handleRectTransform;
        [Header("Sprite references")]
        [SerializeField] private Sprite _moveIcon;
        [SerializeField] private Sprite _moveBackground;
        [SerializeField] private Sprite _rotateIcon;
        [SerializeField] private Sprite _rotateBackground;
        
    }
}
