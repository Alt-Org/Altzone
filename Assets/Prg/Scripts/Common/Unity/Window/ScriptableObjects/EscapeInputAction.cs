using UnityEngine;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Common.Unity.Window.ScriptableObjects
{
    /// <summary>
    /// New Input System Package <c>InputActionReference</c> configuration for ESCAPE or BACK "key" action.
    /// </summary>
    /// <remarks>
    /// For example in "DefaultInputActions" asset its path is "UI/Cancel" for this action.
    /// </remarks>
    //[CreateAssetMenu(menuName = "ALT-Zone/EscapeInputAction", fileName = "EscapeInputAction")]
    public class EscapeInputAction : ScriptableObject
    {
        [Header("Input System Package")] public InputActionReference _escapeInputAction;
    }
}