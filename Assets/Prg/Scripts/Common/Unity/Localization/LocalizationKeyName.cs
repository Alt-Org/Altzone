using UnityEngine;

namespace Prg.Scripts.Common.Unity.Localization
{
    /// <summary>
    /// Localization key holder.
    /// </summary>
    /// <remarks>
    /// Can be used for manual localization if required.
    /// </remarks>
    public class LocalizationKeyName : MonoBehaviour
    {
        [Header("Localization"), SerializeField] protected string _localizationKey;

        public string LocalizationKey
        {
            get => _localizationKey;
            set => _localizationKey = value;
        }
    }
}