using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InRoom
{
    /// <summary>
    /// Prepares players in a room for the game play.
    /// </summary>
    public class RoomSetupManager : MonoBehaviour, IInRoomCallbacks
    {
        private const string PlayerPositionKey = PhotonBattle.PlayerPositionKey;
        private const string PlayerMainSkillKey = PhotonBattle.PlayerMainSkillKey;

        private const int PlayerPositionGuest = PhotonBattle.PlayerPositionGuest;
        private const int PlayerPosition1 = PhotonBattle.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattle.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattle.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattle.PlayerPosition4;
        private const int PlayerPositionSpectator = PhotonBattle.PlayerPositionSpectator;

        private const string TeamBlueKey = PhotonBattle.TeamBlueKey;
        private const string TeamRedKey = PhotonBattle.TeamRedKey;
        private const int TeamBlueValue = PhotonBattle.TeamBlueValue;
        private const int TeamRedValue = PhotonBattle.TeamRedValue;

        [Header("Settings"), SerializeField] private Text _upperTeamText;
        [SerializeField] private Text _lowerTeamText;
        [SerializeField] private Button _buttonPlayerP1;
        [SerializeField] private Button _buttonPlayerP2;
        [SerializeField] private Button _buttonPlayerP3;
        [SerializeField] private Button _buttonPlayerP4;
        [SerializeField] private Button _buttonGuest;
        [SerializeField] private Button _buttonSpectator;
        [SerializeField] private Button _buttonStartPlay;

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

        private void OnEnable()
        {
            _buttonPlayerP1.interactable = false;
            _buttonPlayerP2.interactable = false;
            _buttonPlayerP3.interactable = false;
            _buttonPlayerP4.interactable = false;
            _buttonGuest.interactable = false;
            _buttonSpectator.interactable = false;
            _buttonStartPlay.interactable = false;
            if (!PhotonNetwork.InRoom)
            {
                return;
            }
            // Reset player custom properties for new game
            var player = PhotonNetwork.LocalPlayer;
            player.CustomProperties.Clear();
            // Guest by default
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            var defence = playerDataCache.CharacterModel.MainDefence;
            player.SetCustomProperties(new Hashtable
            {
                { PlayerPositionKey, PlayerPositionGuest },
                { PlayerMainSkillKey, (int)defence }
            });
            if (player.IsMasterClient)
            {
                var room = PhotonNetwork.CurrentRoom;
                room.SetCustomProperties(new Hashtable
                {
                    // Master client plays in Team Blue "Alpha"
                    { TeamBlueKey, "Alpha" },
                    { TeamRedKey, "Beta" }
                });
            }
            UpdateStatus();
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void UpdateStatus()
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }
            ResetState();
            // We need local player to check against other players
            var localPLayer = PhotonNetwork.LocalPlayer;
            _localPlayerPosition = localPLayer.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
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
            // NOTE that in real world we would not use this kind of protocol - but one with more precise logic for handshaking!
            if (PhotonNetwork.IsMasterClient)
            {
                // While in the game room that is going to start playing soon
                // we will send known runtime settings every time something significant changes, regardless of what to be on the safe side :)
                GameConfigSynchronizer.Synchronize(What.All);
            }
            else
            {
                GameConfigSynchronizer.Listen();
            }
        }

        private void SetTeamText()
        {
            var room = PhotonNetwork.CurrentRoom;
            var masterTeam = GetTeam(_masterClientPosition);
            if (masterTeam == 0)
            {
                _upperTeamText.text = $"Team {room.GetCustomProperty<string>(TeamRedKey)}";
                _lowerTeamText.text = $"Team {room.GetCustomProperty<string>(TeamBlueKey)}";
            }
            else
            {
                _upperTeamText.text = $"Team {room.GetCustomProperty<string>(TeamBlueKey)}";
                _lowerTeamText.text = $"Team {room.GetCustomProperty<string>(TeamRedKey)}";
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
                    _interactablePlayerP1 = false;
                    _captionPlayerP1 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition2:
                    _interactablePlayerP2 = false;
                    _captionPlayerP2 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition3:
                    _interactablePlayerP3 = false;
                    _captionPlayerP3 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition4:
                    _interactablePlayerP4 = false;
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

        private void ResetState()
        {
            _interactablePlayerP1 = true;
            _interactablePlayerP2 = true;
            _interactablePlayerP3 = true;
            _interactablePlayerP4 = true;
            _interactableGuest = true;
            _interactableSpectator = true;
            _interactableStartPlay = false;

            _captionPlayerP1 = $"Player {PlayerPosition1}";
            _captionPlayerP2 = $"Player {PlayerPosition2}";
            _captionPlayerP3 = $"Player {PlayerPosition3}";
            _captionPlayerP4 = $"Player {PlayerPosition4}";
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