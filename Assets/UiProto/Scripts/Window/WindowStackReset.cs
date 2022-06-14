using UnityEngine;

namespace UiProto.Scripts.Window
{
    /// <summary>
    /// Helper to remove <c>UiProto</c> <c>WindowStack</c> in order to prevent double ESCAPE handlers running one after an other.< br/>
    /// <c>WindowStack</c> is created again if old <c>UiProto</c> gets visible.
    /// </summary>
    public class WindowStackReset : MonoBehaviour
    {
        private void OnEnable()
        {
            throw new UnityException($"{nameof(WindowStackReset)} is obsolete");
        }
    }
}