using UnityEngine;

namespace Prg.Scripts.Common.Unity.Input
{
    /// <summary>
    /// Convenience class to create <c>InputManagerConfig</c> in Editor and drag to <c>InputManager</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "ALT-Zone/" + nameof(InputManagerConfig))]
    public class InputManagerConfig : ScriptableObject
    {
        [Header("Mouse")] public InputManager.ZoomAndPan _mouse;

        [Header("Touch")] public InputManager.ZoomAndPan _touch;
    }
}