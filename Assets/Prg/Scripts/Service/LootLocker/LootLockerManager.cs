#if USE_LOOTLOCKER
using System;
using System.Threading.Tasks;
using LootLocker;
using LootLocker.Requests;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Service.LootLocker
{
    [Serializable]
    public class PlayerHandle
    {
        [SerializeField] private string _localPlayerGuid;
        [SerializeField] private string _playerName;
        [SerializeField] private int _lootLockerPlayerID;

        public string LocalPlayerGuid => _localPlayerGuid;
        public int LootLockerPlayerID => _lootLockerPlayerID;
        public string PlayerName => _playerName;

        public PlayerHandle(string localPlayerGuid, string playerName, int lootLockerPlayerID = -1)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(playerName), "string.IsNullOrWhiteSpace(playerName)");
            _localPlayerGuid = localPlayerGuid;
            _playerName = playerName;
            _lootLockerPlayerID = lootLockerPlayerID;
        }
    }

    /// <summary>
    /// Asynchronous LootLocker API manager.
    /// </summary>
    /// <remarks>
    /// We create a new session and try to synchronize player name between <c>LootLocker</c> and our <c>PlayerPrefs</c>.<br />
    /// We mostly ignore errors as next time player logins there is a chance to fix everything that needs to be fixed.
    /// </remarks>
    public class LootLockerManager
    {
        private const LootLockerConfig.platformType Platform = LootLockerConfig.platformType.Android;

        public static Func<string> ApiKey;

        private PlayerHandle _playerHandle;
        private bool _isStartSessionReady;

        public PlayerHandle PlayerHandle => _playerHandle;
        public bool IsRunning => _isStartSessionReady && _playerHandle != null;

        public void Init(string gameVersion, string apiKey, bool onDevelopmentMode, string domainKey)
        {
            Debug.Log($"LootLockerSDKManager.Init mode {(onDevelopmentMode ? "DEV" : "PROD")}");
            var success = LootLockerSDKManager.Init(apiKey, gameVersion, Platform, onDevelopmentMode, domainKey);
            Debug.Log($"Init success {success}");
            _isStartSessionReady = false;
        }

        public async void Run(string localPlayerGuid, string playerName, Action<string> setPlayerName)
        {
            Debug.Log($"playerName {playerName}");
            _isStartSessionReady = false;
            var success = await StartSession(localPlayerGuid, playerName, setPlayerName);
            _isStartSessionReady = success;
            Debug.Log("done");
        }

        private async Task<bool> StartSession(string localPlayerGuid, string playerName, Action<string> updatePlayerName)
        {
            Debug.Log($"playerName {playerName} guid {localPlayerGuid}");
            var sessionResp = await LootLockerAsync.StartSession(localPlayerGuid);
            if (!sessionResp.success)
            {
                Debug.Log($"sessionResp {sessionResp.text}");
                // Create dummy player using PlayerPrefs values
                _playerHandle = new PlayerHandle(localPlayerGuid, playerName);
                return false;
            }
            _playerHandle = new PlayerHandle(localPlayerGuid, playerName, sessionResp.player_id);
            Debug.Log($"sessionResp uid {sessionResp.public_uid} '{_playerHandle.PlayerName}'");

            if (!sessionResp.seen_before)
            {
                // This is new player
                Debug.Log($"SetPlayerName (NOT seen_before) '{_playerHandle.PlayerName}'");
                var task = LootLockerAsync.SetPlayerName(_playerHandle.PlayerName); // Fire and forget
                return true;
            }

            var getNameResp = await LootLockerAsync.GetPlayerName();
            if (!getNameResp.success || string.IsNullOrWhiteSpace(getNameResp.name))
            {
                // Failed to get or name is empty
                Debug.Log($"getNameResp {getNameResp.text}");
                Debug.Log($"SetPlayerName '{_playerHandle.PlayerName}'");
                var task = LootLockerAsync.SetPlayerName(_playerHandle.PlayerName); // Fire and forget
                return true;
            }
            if (_playerHandle.PlayerName != getNameResp.name)
            {
                // Update local name from LootLocker
                Debug.Log($"UpdatePlayerName '{playerName}' <- '{_playerHandle.PlayerName}'");
                _playerHandle = new PlayerHandle(localPlayerGuid, _playerHandle.PlayerName, sessionResp.player_id);
                updatePlayerName?.Invoke(_playerHandle.PlayerName);
            }
            return true;
        }

        public async Task SetPlayerName(string playerName, Action<string> setPlayerName)
        {
            var setNameResp = await LootLockerAsync.SetPlayerName(playerName);
            if (setNameResp.success)
            {
                Debug.Log($"Update player {_playerHandle.PlayerName} <- {setNameResp.name}");
                _playerHandle = new PlayerHandle(_playerHandle.LocalPlayerGuid, setNameResp.name, _playerHandle.LootLockerPlayerID);
            }
            setPlayerName?.Invoke(_playerHandle.PlayerName);
        }
    }
}
#endif