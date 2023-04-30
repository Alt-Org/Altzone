using UnityEngine;

namespace Altzone.Scripts.Settings
{
    /// <summary>
    /// Player settings - a common storage for player related data that is persisted locally on the device.
    /// </summary>
    /// <remarks>
    /// See https://github.com/Alt-Org/Altzone/wiki/Battle-Pelihahmo
    /// </remarks>
    public interface IPlayerSettings
    {
        string PlayerGuid { get; }
        string PhotonRegion { get; set; }
        SystemLanguage Language { get; set; }
        bool IsDebugFlag { get; set; }
        bool IsTosAccepted { get; set; }
        bool IsFirstTimePlaying { get; set; }
        bool IsAccountVerified { get; set; }
        int PlayerSettingsVersion { get; }

        void SetPlayerGuid(string newPlayerGuid);

#if UNITY_EDITOR
        void ResetPlayerSettings();
        void SetLanguageToEnglish();
#endif
    }
}