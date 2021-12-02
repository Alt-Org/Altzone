using UnityEngine;

namespace Altzone.Scripts.Config
{
    [CreateAssetMenu(menuName = "ALT-Zone/FolderConfig", fileName = "FolderConfig")]
    public class FolderConfig : ScriptableObject
    {
        /// <summary>
        /// Custom location for project settings
        /// </summary>
        [Header("Config Folders")] public string primaryConfigFolder;
    }
}