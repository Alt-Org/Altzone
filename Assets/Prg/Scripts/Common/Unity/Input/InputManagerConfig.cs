using UnityEngine;

namespace Prg.Scripts.Common.Unity.Input
{
    /// <summary>
    /// Convenience class to create <c>InputManagerConfig</c> in Editor and drag to <c>InputManager</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "ALT-Zone/" + nameof(InputManagerConfig))]
    public class InputManagerConfig : ScriptableObject
    {
        [Header("Mouse")] public InputManager.ZoomAndPanSettings _mouse;

        [Header("Touch")] public InputManager.ZoomAndPanSettings _touch;
    }
}