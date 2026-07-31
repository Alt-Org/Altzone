using UnityEngine;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// Local development settings.
    /// </summary>
    /// <remarks>
    /// There should be at most one instance of these, if you want to override some default settings for the project during development.
    /// <br></br>
    /// Do not save these in version control system.
    /// </remarks>
    [CreateAssetMenu(menuName = "ALT-Zone/LocalDevConfig", fileName = "LocalDevConfig")]
    public class LocalDevConfig : ScriptableObject
    {
        /// <summary>
        /// Override Application.targetFrameRate for Game View in UNITY Editor, default value is -1 to use.
        /// </summary>
        [Header("Editor Settings"), Range(-1, 999)] public int _targetFrameRateOverride = -1;
    }
}
