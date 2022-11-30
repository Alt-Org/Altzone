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

        public PlayerHandle PlayerHandle => _playerHandle;
        public bool IsRunning => _playerHandle?.LootLockerPlayerID > 0;

        public void Init(string gameVersion, string apiKey, bool isDevelopmentMode, string domainKey)
        {
            // NOTE that LootLocker must be properly initialized every time it is used because of the way it is implemented with GameObjects!
            var config = LootLockerConfig.Get();
            var version = config.dateVersion;
            Debug.Log($"{gameVersion} mode {(isDevelopmentMode ? "DEV" : "PROD")} : {version}");
            var success = LootLockerSDKManager.Init(apiKey, gameVersion, Platform, isDevelopmentMode, domainKey);
            Debug.Log($"Init success {success}");
        }

        public async void StartSessionAsync(string localPlayerGuid, string playerName, Action<string> setPlayerName)
        {
            Debug.Log($"playerName {playerName}");
            var startTime = Time.time;
            var success = await StartSession(localPlayerGuid, playerName, setPlayerName);
            Debug.Log($"done success {success} in {Time.time - startTime:0.00} s");
        }

        public void EndSession()
        {
            var deviceID = LootLockerConfig.Get().deviceID;
            Debug.Log($"deviceID {deviceID} IsRunning {IsRunning}");
            LootLockerSDKManager.EndSession((sessionResp) =>
            {
                Debug.Log($"sessionResp {sessionResp.text}");
                deviceID = LootLockerConfig.Get().deviceID;
                Debug.Log($"done {deviceID} IsRunning {IsRunning}");
            });
        }

        private async Task<bool> StartSession(string localPlayerGuid, string playerName, Action<string> updatePlayerName)
        {
            Debug.Log($"playerName '{playerName}' guid {localPlayerGuid}");
            var sessionResp = await LootLockerAsync.StartSession(localPlayerGuid);
            if (!sessionResp.success)
            {
                Debug.Log($"sessionResp {sessionResp.text}");
                // Create dummy player using PlayerPrefs values.
                _playerHandle = new PlayerHandle(localPlayerGuid, playerName);
                return false;
            }
            // Create valid player for this session.
            _playerHandle = new PlayerHandle(localPlayerGuid, playerName, sessionResp.player_id);
            Debug.Log($"sessionResp '{_playerHandle.PlayerName}' uid {sessionResp.public_uid} seen_before {sessionResp.seen_before}");

            // Validate (synchronize) player name between local device settings and LootLocker.
            if (!sessionResp.seen_before)
            {
                // This is new player - set LootLocker player name.
                Debug.Log($"SetPlayerName (NOT seen_before) '{_playerHandle.PlayerName}'");
                var task = LootLockerAsync.SetPlayerName(_playerHandle.PlayerName); // Fire and forget
                return true;
            }

            var getNameResp = await LootLockerAsync.GetPlayerName();
            if (!getNameResp.success || string.IsNullOrWhiteSpace(getNameResp.name))
            {
                // Failed to get LootLocker player name or it is empty.
                Debug.Log($"getNameResp {getNameResp.text}");
                Debug.Log($"SetPlayerName '{_playerHandle.PlayerName}'");
                var task = LootLockerAsync.SetPlayerName(_playerHandle.PlayerName); // Fire and forget
                return true;
            }
            if (_playerHandle.PlayerName != getNameResp.name)
            {
                // Update our local player name from LootLocker.
                Debug.Log($"UpdatePlayerName '{playerName}' <- '{_playerHandle.PlayerName}'");
                _playerHandle = new PlayerHandle(localPlayerGuid, _playerHandle.PlayerName, sessionResp.player_id);
                updatePlayerName?.Invoke(_playerHandle.PlayerName);
            }
            return true;
        }

        public async Task SetPlayerNameAsync(string playerName, Action<string> setPlayerName)
        {
            // Even if LootLocker fails we should save the new player name to local settings.
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