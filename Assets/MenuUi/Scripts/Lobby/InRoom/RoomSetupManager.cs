using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonBattle = Altzone.Scripts.Battle.Photon.PhotonBattleRoom;

namespace MenuUI.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Prepares players in a room for the game play.
    /// </summary>
    public class RoomSetupManager : MonoBehaviour, IInRoomCallbacks
    {
        private const string PlayerPositionKey = PhotonBattle.PlayerPositionKey;
        private const string PlayerMainSkillKey = PhotonBattle.PlayerPrefabIdKey;
        private const string PlayerCharactersKey = PhotonBattle.PlayerPrefabIdsKey;
        private const string PlayerStatsKey = PhotonBattle.PlayerStatsKey;

        private const int PlayerPositionGuest = PhotonBattle.PlayerPositionGuest;
        private const int PlayerPosition1 = PhotonBattle.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattle.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattle.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattle.PlayerPosition4;
        private const int PlayerPositionSpectator = PhotonBattle.PlayerPositionSpectator;

        private const string TeamBlueNameKey = PhotonBattle.TeamAlphaNameKey;
        private const string TeamRedNameKey = PhotonBattle.TeamBetaNameKey;
        private const int TeamBlueValue = PhotonBattle.TeamAlphaValue;
        private const int TeamRedValue = PhotonBattle.TeamBetaValue;

        [Header("Settings"), SerializeField] private Text _upperTeamText;
        [SerializeField] private Text _lowerTeamText;
        [SerializeField] private Button _buttonPlayerP1;
        [SerializeField] private Button _buttonPlayerP2;
        [SerializeField] private Button _buttonPlayerP3;
        [SerializeField] private Button _buttonPlayerP4;
        [SerializeField] private Button _buttonGuest;
        [SerializeField] private Button _buttonSpectator;
        [SerializeField] private Button _buttonStartPlay;
        [SerializeField] private Button _buttonRaidTest;

        [Header("Live Data"), SerializeField] private int _localPlayerPosition;
        [SerializeField] private bool _isLocalPlayerPositionUnique;
        [SerializeField] private int _masterClientPosition;


        private bool _interactablePlayerP1;
        private bool _interactablePlayerP2;
        private bool _interactablePlayerP3;
        private bool _interactablePlayerP4;
        private bool _interactableGuest;
        private bool _interactableSpectator;
        private bool _interactableStartPlay;

        private string _captionPlayerP1;
        private string _captionPlayerP2;
        private string _captionPlayerP3;
        private string _captionPlayerP4;
        private string _captionGuest;
        private string _captionSpectator;

        PlayerRole currentRole = PlayerRole.Player;

        public enum PlayerRole
        {
            Player,
            Spectator
        }

        private void OnEnable()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
            _buttonPlayerP1.interactable = false;
            _buttonPlayerP2.interactable = false;
            _buttonPlayerP3.interactable = false;
            _buttonPlayerP4.interactable = false;
            _buttonGuest.interactable = false;
            _buttonSpectator.interactable = false;
            _buttonStartPlay.interactable = false;
            _buttonRaidTest.interactable = false;

            PhotonNetwork.AddCallbackTarget(this);
            StartCoroutine(OnEnableInRoom());
        }

        private void OnDisable()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private IEnumerator OnEnableInRoom()
        {
            yield return new WaitUntil(() => PhotonNetwork.InRoom);

            UpdatePhotonNickname();
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"OnEnable InRoom '{room.Name}' as '{PhotonNetwork.NickName}'");

            // Reset player custom properties for new game
            player.CustomProperties.Clear();
            var playerPos = PhotonBattle.GetFirstFreePlayerPos(player);
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                var battleCharacter = playerData.CurrentBattleCharacters;
                Debug.Log($"{battleCharacter[0]}");
                int[] characterIds = new int[5];
                float[] characterStats = new float[25];
                for (int i = 0; i < 5; i++)
                {
                    characterIds[i] = (int)battleCharacter[i].Id;
                    characterStats[i * 5] = battleCharacter[i].Hp;
                    characterStats[i * 5 + 1] = battleCharacter[i].Speed;
                    characterStats[i * 5 + 2] = battleCharacter[i].Resistance;
                    characterStats[i * 5 + 3] = battleCharacter[i].Attack;
                    characterStats[i * 5 + 4] = battleCharacter[i].Defence;
                }

                //var prefabIndex = PhotonBattle.GetPrefabIndex(battleCharacter[0], 0);
                var prefabIndex = (int)battleCharacter[0].Id;
                Debug.Log($"playerPos {playerPos} prefabIndex {characterIds}");
                player.SetCustomProperties(new Hashtable
                {
                    { PlayerPositionKey, playerPos },
                    { PlayerMainSkillKey, prefabIndex },
                    { PlayerCharactersKey, characterIds },
                    { PlayerStatsKey, characterStats },
                    { "Role", (int)currentRole }
                });
                Debug.Log($"{PhotonNetwork.NetworkClientState} {enabled}");
                UpdateStatus();
            });
        }

        private void UpdateStatus()
        {
            if (!enabled || !PhotonNetwork.InRoom)
            {
                return;
            }
            ResetState();
            // We need local player to check against other players
            var localPLayer = PhotonNetwork.LocalPlayer;
            _localPlayerPosition = localPLayer.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            //currentRole = PlayerRole.Player;
            _isLocalPlayerPositionUnique = true;

            CheckMasterClient();
            // Check other players first is they have reserved some player positions etc. from the room already.
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (!player.Equals(localPLayer))
                {
                    CheckOtherPlayer(player);
                }
            }
            CheckLocalPlayer(localPLayer);

            SetButton(_buttonPlayerP1, _interactablePlayerP1, _captionPlayerP1);
            SetButton(_buttonPlayerP2, _interactablePlayerP2, _captionPlayerP2);
            SetButton(_buttonPlayerP3, _interactablePlayerP3, _captionPlayerP3);
            SetButton(_buttonPlayerP4, _interactablePlayerP4, _captionPlayerP4);
            SetButton(_buttonGuest, _interactableGuest, _captionGuest);
            SetButton(_buttonSpectator, _interactableSpectator, _captionSpectator);
            SetButton(_buttonStartPlay, _interactableStartPlay, null);
            SetButton(_buttonRaidTest, _interactableStartPlay, null);

            if (_localPlayerPosition >= 0 && _localPlayerPosition <= 3 &&
                _masterClientPosition >= 0 && _masterClientPosition <= 3)
            {
                _upperTeamText.gameObject.SetActive(true);
                _lowerTeamText.gameObject.SetActive(true);
                SetTeamText();
            }
            else
            {
                _upperTeamText.gameObject.SetActive(false);
                _lowerTeamText.gameObject.SetActive(false);
            }
        }

        private void UpdatePhotonNickname()
        {
            var store = Storefront.Get();
            PlayerData playerData = null;
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
            PhotonNetwork.NickName = playerData.Name;
        }

        private void SetTeamText()
        {
            var room = PhotonNetwork.CurrentRoom;
            var masterTeam = GetTeam(_masterClientPosition);
            if (masterTeam == 0)
            {
                _upperTeamText.text = $"Team {room.GetCustomProperty<string>(TeamRedNameKey)}";
                _lowerTeamText.text = $"Team {room.GetCustomProperty<string>(TeamBlueNameKey)}";
            }
            else
            {
                _upperTeamText.text = $"Team {room.GetCustomProperty<string>(TeamBlueNameKey)}";
                _lowerTeamText.text = $"Team {room.GetCustomProperty<string>(TeamRedNameKey)}";
            }
        }

        private static int GetTeam(int playerPos)
        {
            if (playerPos == PlayerPosition1 || playerPos == PlayerPosition3)
            {
                return TeamBlueValue;
            }
            if (playerPos == PlayerPosition4 || playerPos == PlayerPosition2)
            {
                return TeamRedValue;
            }
            return -1;
        }

        private void CheckMasterClient()
        {
            var curValue = PhotonNetwork.MasterClient.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            _masterClientPosition = curValue;
        }

        private void CheckOtherPlayer(Player player)
        {
            Debug.Log($"checkOtherPlayer {player.GetDebugLabel()}");
            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                return;
            }
            var curValue = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            if (_isLocalPlayerPositionUnique && curValue >= PlayerPosition1 && curValue <= PlayerPosition4)
            {
                if (curValue == _localPlayerPosition)
                {
                    Debug.Log("detected conflict");
                    _isLocalPlayerPositionUnique = false; // Conflict with player positions!
                }
            }
            switch (curValue)
            {
                case PlayerPosition1:
                    _interactablePlayerP1 = false;
                    _captionPlayerP1 = player.NickName;
                    break;
                case PlayerPosition2:
                    _interactablePlayerP2 = false;
                    _captionPlayerP2 = player.NickName;
                    break;
                case PlayerPosition3:
                    _interactablePlayerP3 = false;
                    _captionPlayerP3 = player.NickName;
                    break;
                case PlayerPosition4:
                    _interactablePlayerP4 = false;
                    _captionPlayerP4 = player.NickName;
                    break;
            }
        }

        private void CheckLocalPlayer(Player player)
        {
            Debug.Log($"checkLocalPlayer {player.GetDebugLabel()} pos={_localPlayerPosition} ok={_isLocalPlayerPositionUnique}");
            var curValue = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            // Master client can *only* start the game when in room as player!
            _interactableStartPlay = player.IsMasterClient && curValue >= PlayerPosition1 && curValue <= PlayerPosition4;
            switch (curValue)
            {
                case PlayerPosition1:
                    _captionPlayerP1 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition2:
                    _captionPlayerP2 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition3:
                    _captionPlayerP3 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition4:
                    _captionPlayerP4 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPositionGuest:
                    _interactableGuest = false;
                    _captionGuest = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPositionSpectator:
                    _interactableSpectator = false;
                    _captionSpectator = $"<color=blue>{player.NickName}</color>";
                    break;
            }
        }

        private void ButtonState(int _buttonStateNumber, string _buttonCaption)
        {
            _buttonStateNumber++;
            switch (_buttonStateNumber)
            {
                case 0:
                    _buttonCaption = $"Free";
                    Debug.Log(_buttonCaption);
                    break;

                case 1:
                    _buttonCaption = $"Player Name";
                    Debug.Log(_buttonCaption);
                    break;

                case 2:
                    _buttonCaption = $"Bot";
                    Debug.Log(_buttonCaption);
                    break;

                case 3:
                    _buttonCaption = $"Closed";
                    Debug.Log(_buttonCaption);
                    break;
                case 4: 
                    _buttonStateNumber = 0;
                    break;
            }
        }

        private void ResetState()
        {
            _interactablePlayerP1 = true;
            _interactablePlayerP2 = true;
            _interactablePlayerP3 = true;
            _interactablePlayerP4 = true;
            _interactableGuest = true;
            // Spectator does not work with current NetworkSync and RoomLoader2,
            // it is from earlier versions with different implementation how room was initialized.
            _interactableSpectator = false; //true;
            _interactableStartPlay = false;

            _captionPlayerP1 = $"Free";
            _captionPlayerP2 = $"Free";
            _captionPlayerP3 = $"Free";
            _captionPlayerP4 = $"Free";
            _captionGuest = "Guest";
            _captionSpectator = "Spectator";
        }

        private static void SetButton(Selectable selectable, bool interactable, string caption)
        {
            selectable.interactable = interactable;
            if (!string.IsNullOrEmpty(caption))
            {
                selectable.GetComponentInChildren<Text>().text = caption;
            }
        }

        void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateStatus();
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdateStatus();
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            UpdateStatus();
        }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            UpdateStatus();
        }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
            UpdateStatus();
        }
    }
}
