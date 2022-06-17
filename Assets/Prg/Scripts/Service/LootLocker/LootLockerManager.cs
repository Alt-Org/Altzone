#if USE_LOOTLOCKER
using System;
using System.Threading.Tasks;
using LootLocker;
using LootLocker.Requests;
using UnityEngine;

namespace Prg.Scripts.Service.LootLocker
{
    [Serializable]
    public class PlayerHandle
    {
        [SerializeField] private string _deviceId;
        [SerializeField] private string _playerName;
        [SerializeField] private int _playerID;

        public string DeviceId => _deviceId;
        public int PlayerId => _playerID;
        public string PlayerName => _playerName;

        public PlayerHandle(string deviceId, string playerName, int playerId)
        {
            this._deviceId = deviceId;
            this._playerName = playerName;
            _playerID = playerId;
        }
    }

    /// <summary>
    /// Test driver for LootLocker Game BAAS
    /// </summary>
    /// <remarks>
    /// We create a new session and try to synchronize player name between <c>LootLocker</c> and our <c>PlayerPrefs</c>.<br />
    /// We mostly ignore errors as next time player logins there is a chance to fix everything that need to be fixed.
    /// </remarks>
    public class LootLockerManager : MonoBehaviour
    {
        private const string DoNotSaveItHere = "f1e477e40a312095f53887ebb3de4425b19e420a";

        [SerializeField] private bool _isAsyncMode;
        [SerializeField] private PlayerHandle _playerHandle;
        [SerializeField] private bool _isStartSessionReady;

        public PlayerHandle PlayerHandle => _playerHandle;
        public bool IsValid => _playerHandle != null && _isStartSessionReady;

        /// <summary>
        /// Asynchronous methods can be mind-boggling even though they make life a lot easier!
        /// </summary>
        private void Awake()
        {
            Debug.Log("start");
            var apiKey = DoNotSaveItHere;
            var gameVersion = "0.0.0.1";
            var platform = LootLockerConfig.platformType.Android;
            LootLockerSDKManager.Init(apiKey, gameVersion, platform, true, null);
            _isStartSessionReady = false;
        }

        public async void Init(string deviceId, string playerName, Action<string> setPlayerName)
        {
            Debug.Log("start");
            _isStartSessionReady = false;
            if (_isAsyncMode)
            {
                await AsyncInit(deviceId, playerName, setPlayerName);
                _isStartSessionReady = true;
                Debug.Log("done");
            }
            else
            {
                CallbackInit(deviceId, playerName, setPlayerName);
            }
        }
        private async Task AsyncInit(string deviceId, string playerName, Action<string> setPlayerName)
        {
            Debug.Log($"StartSession for {playerName} {deviceId}");
            var sessionResp = await LootLockerAsync.StartSession(deviceId);
            if (!sessionResp.success)
            {
                if (sessionResp.text.Contains("Game not found"))
                {
                    Debug.LogError("INVALID game_key");
                }
                // Create dummy player using PlayerPrefs values
                _playerHandle = new PlayerHandle(deviceId, playerName, -1);
                return;
            }
            _playerHandle = new PlayerHandle(deviceId, playerName, sessionResp.player_id);

            if (!sessionResp.seen_before)
            {
                // This is new player
                Debug.Log($"SetPlayerName is NEW '{_playerHandle.PlayerName}'");
                var task = LootLockerAsync.SetPlayerName(_playerHandle.PlayerName); // Fire and forget
                return;
            }

            var getNameResp = await LootLockerAsync.GetPlayerName();
            if (!getNameResp.success || string.IsNullOrWhiteSpace(getNameResp.name))
            {
                // Failed to get or name is empty
                Debug.Log($"SetPlayerName '{_playerHandle.PlayerName}'");
                var task = LootLockerAsync.SetPlayerName(_playerHandle.PlayerName); // Fire and forget
                return;
            }
            if (_playerHandle.PlayerName != getNameResp.name)
            {
                // Update local name from LootLocker
                _playerHandle = new PlayerHandle(deviceId, _playerHandle.PlayerName, sessionResp.player_id);
                setPlayerName?.Invoke(_playerHandle.PlayerName);
            }
        }

        private void CallbackInit(string deviceId, string playerName, Action<string> setPlayerName)
        {
            Debug.Log($"StartSession for {playerName} {deviceId}");
            LootLockerSDKManager.StartSession(deviceId, (sessionResp) =>
            {
                if (!sessionResp.success)
                {
                    _playerHandle = new PlayerHandle(deviceId, playerName, 0);
                    _isStartSessionReady = true;
                    Debug.Log("done");
                    return;
                }
                _playerHandle = new PlayerHandle(deviceId, playerName, sessionResp.player_id);
                if (!sessionResp.seen_before)
                {
                    Debug.Log($"SetPlayerName '{playerName}'");
                    LootLockerSDKManager.SetPlayerName(playerName, null); // Fire and forget
                    _isStartSessionReady = true;
                    Debug.Log("done");
                    return;
                }
                LootLockerSDKManager.GetPlayerName(getNameResp =>
                {
                    if (!getNameResp.success)
                    {
                        _isStartSessionReady = true;
                        Debug.Log("done");
                        return;
                    }
                    if (getNameResp.name == _playerHandle.PlayerName)
                    {
                        _isStartSessionReady = true;
                        Debug.Log("done");
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(getNameResp.name))
                    {
                        _playerHandle = new PlayerHandle(deviceId, getNameResp.name, sessionResp.player_id);
                        setPlayerName?.Invoke(_playerHandle.PlayerName);
                        _isStartSessionReady = true;
                        Debug.Log("done");
                        return;
                    }
                    // Name was empty
                    Debug.Log($"SetPlayerName '{_playerHandle.PlayerName}'");
                    LootLockerSDKManager.SetPlayerName(_playerHandle.PlayerName, setNameResp =>
                    {
                        if (!setNameResp.success || string.IsNullOrWhiteSpace(setNameResp.name))
                        {
                            _isStartSessionReady = true;
                            Debug.Log("done");
                            return;
                        }
                        _playerHandle = new PlayerHandle(deviceId, setNameResp.name, sessionResp.player_id);
                        setPlayerName?.Invoke(_playerHandle.PlayerName);
                        _isStartSessionReady = true;
                        Debug.Log("done");
                    });
                });
            });
        }

        public async Task SetPlayerName(string playerName, Action<string> setPlayerName)
        {
            var setNameResp = await LootLockerAsync.SetPlayerName(playerName);
            if (setNameResp.success)
            {
                Debug.Log($"Update player {_playerHandle.PlayerName} <- {setNameResp.name}");
                _playerHandle = new PlayerHandle(_playerHandle.DeviceId, setNameResp.name, _playerHandle.PlayerId);
            }
            setPlayerName?.Invoke(_playerHandle.PlayerName);
        }
    }
}
#endif