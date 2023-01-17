using UnityEngine;

namespace Altzone.Scripts.Config
{
    /// <summary>
    /// Player data cache - a common storage for player related data (settings) that is persisted locally on the device.
    /// </summary>
    /// <remarks>
    /// See https://github.com/Alt-Org/Altzone/wiki/Battle-Pelihahmo
    /// </remarks>
    public interface IPlayerDataCache
    {
        string PlayerName { get; }
        string PlayerGuid { get; }
        int ClanId { get; }
        int CustomCharacterModelId { get; }
        SystemLanguage Language { get; set; }
        bool IsDebugFlag { get; set; }
        bool IsTosAccepted { get; set; }
        bool IsFirstTimePlaying { get; set; }
        bool IsAccountVerified { get; set; }

        bool HasPlayerName { get; }
        void SetPlayerName(string playerName);
        void SetPlayerGuid(string newPlayerGuid);
        void SetClanId(int clanId);
        void SetCustomCharacterModelId(int customCharacterModelId);

#if UNITY_EDITOR
        void DebugSavePlayer();
        void DebugResetPlayer();
#endif
    }
}