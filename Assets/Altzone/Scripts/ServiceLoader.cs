using Altzone.Scripts.Service.Audio;
using Prg.Scripts.Common.Unity.Localization;
using Prg.Scripts.Common.Unity.Window;
using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Loads all services used by this game.
    /// </summary>
    public class ServiceLoader : MonoBehaviour
    {
        private void OnEnable()
        {
            Debug.Log($"{name}");
            Localizer.LoadTranslations();
            AudioManager.Get();
            // Start the UI now.
            WindowManager.Get();
        }
    }
}