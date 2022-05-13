using Prg.Scripts.Common.Util;
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
        /// Use local Photon version to keep all rooms "invisible" for other developers
        /// </summary>
        [Header("Photon Settings")] public string _photonVersionOverride;

        /// <summary>
        /// Reference to <c>LoggerConfig</c> to use.
        /// </summary>
        [Header("Development Settings")] public LoggerConfig _loggerConfig;
    }
}