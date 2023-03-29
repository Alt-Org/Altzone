using System;
using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// Convenience class to set UNITY scene name in Editor without typos using custom <c>PropertyDrawer</c>.
    /// </summary>
    [Serializable]
    public class UnitySceneName
    {
        /// <summary>
        /// UNITY scene name without path.
        /// </summary>
        public string sceneName;

        /// <summary>
        /// Scene GUID is currently not used, but just saved if need to track actual scene by its GUID arises.
        /// </summary>
        [SerializeField] private string sceneGuid;
    }
}