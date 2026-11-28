using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Config;
using Altzone.Scripts.Language;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Displays the full custom-room setup, including teams, bots and four player slots.
    /// </summary>
    public class RoomSetupManager : RoomSetupBase
    {

        private const string TeamAlphaNameKey = PhotonBattleRoom.TeamAlphaNameKey;
        private const string TeamBetaNameKey = PhotonBattleRoom.TeamBetaNameKey;
        private const int TeamAlphaValue = PhotonBattleRoom.TeamAlphaValue;
        private const int TeamBetaValue = PhotonBattleRoom.TeamBetaValue;

        [Header("Settings"), SerializeField] private TextMeshProUGUI _upperTeamText;
        [SerializeField] private TextMeshProUGUI _lowerTeamText;
        [SerializeField] private SliderToggle _toggleBotFill;
        [SerializeField] private InRoomPlayerSlot _player1Slot;
        [SerializeField] private InRoomPlayerSlot _player2Slot;
        [SerializeField] private InRoomPlayerSlot _player3Slot;
        [SerializeField] private InRoomPlayerSlot _player4Slot;

        [Header("Live Data"), SerializeField] private int _masterClientPosition;
        private bool _isPlayerMaster;

        private bool _interactablePlayerP1;
        private bool _interactablePlayerP2;
        private bool _interactablePlayerP3;
        private bool _interactablePlayerP4;
        private bool _interactableStartPlay;

        private bool _player1SlotInUse = false;
        private bool _player2SlotInUse = false;
        private bool _player3SlotInUse = false;
        private bool _player4SlotInUse = false;

        private RectTransform _player1Rect;
        private RectTransform _player3Rect;
        private Vector2 _player1AnchorMin;
        private Vector2 _player1AnchorMax;
        private Vector2 _player3AnchorMin;
        private Vector2 _player3AnchorMax;
        private bool _oneVsOnePositionsInitialized;
        private bool _oneVsOneLayoutActive;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_toggleBotFill != null) _toggleBotFill.onValueChanged.AddListener(SetFillBotToggle);
        }

        protected override void OnDisable()
        {
            if (_toggleBotFill != null) _toggleBotFill.onValueChanged.RemoveListener(SetFillBotToggle);
            base.OnDisable();
        }

        protected override void InitializeSlots()
        {
            if (_player1Slot) _player1Slot.Initialize(0);
            if (_player2Slot) _player2Slot.Initialize(1);
            if (_player3Slot) _player3Slot.Initialize(2);
            if (_player4Slot) _player4Slot.Initialize(3);
        }

        protected override void UpdateStatus()
        {
            if (!enabled || !PhotonRealtimeClient.InRoom)
            {
                return;
            }
            ResetState();
            MatchmakingType roomGameType = (MatchmakingType)PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey);
            if(roomGameType is MatchmakingType.Custom)
            {
                bool oneVsOne = PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers == 2;
                if (oneVsOne)
                {
                    SetPlayerSlotLayout(true);
                    if (_player1Slot != null) _player1Slot.gameObject.SetActive(true);
                    if (_player2Slot != null) _player2Slot.gameObject.SetActive(false);
                    if (_player3Slot != null) _player3Slot.gameObject.SetActive(true);
                    if (_player4Slot != null) _player4Slot.gameObject.SetActive(false);
                }
                else
                {
                    if (_player1Slot != null) _player1Slot.gameObject.SetActive(true);
                    if (_player2Slot != null) _player2Slot.gameObject.SetActive(true);
                    if (_player3Slot != null) _player3Slot.gameObject.SetActive(true);
                    if (_player4Slot != null) _player4Slot.gameObject.SetActive(true);
                    SetPlayerSlotLayout(false);
                }
            }

            // We need local player to check against other players
            LobbyPlayer localPlayer = PhotonRealtimeClient.LocalLobbyPlayer;

            CheckMasterClient();

            // Check if bot fill is active
            bool botFillActive = PhotonBattleRoom.IsBotFillActive();
            if (_toggleBotFill != null) _toggleBotFill.SetState(botFillActive);

            //Check if positions have bots
            bool botActive1 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition1);
            if (botActive1)
            {
                _player1Slot.SetBotCharacters(_isPlayerMaster);
                _player1SlotInUse = true;
            }
            bool botActive2 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition2);
            if (botActive2)
            {
                _player2Slot.SetBotCharacters(_isPlayerMaster);
                _player2SlotInUse = true;
            }
            bool botActive3 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition3);
            if (botActive3)
            {
                _player3Slot.SetBotCharacters(_isPlayerMaster);
                _player3SlotInUse = true;
            }
            bool botActive4 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition4);
            if (botActive4)
            {
                _player4Slot.SetBotCharacters(_isPlayerMaster);
                _player4SlotInUse = true;
            }

            // Check other players first is they have reserved some player positions etc. from the room already.
            foreach (var player in PhotonRealtimeClient.GetCurrentRoomPlayers())
            {
                if (!player.Equals(localPlayer))
                {
                    CheckOtherPlayer(player);
                }
            }
            CheckLocalPlayer(localPlayer);

            // Setting start game button interactable status
            if (_buttonStartPlay != null) _buttonStartPlay.interactable = _interactableStartPlay;

            if (_player1Slot && !_player1SlotInUse) _player1Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player2Slot && !_player2SlotInUse) _player2Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player3Slot && !_player3SlotInUse) _player3Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player4Slot && !_player4SlotInUse) _player4Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);

            // Setting team text
            SetTeamText();
        }

        private void SetPlayerSlotLayout(bool oneVsOne)
        {
            if (oneVsOne == _oneVsOneLayoutActive) return;

            if (_player1Rect == null && _player1Slot != null)
                _player1Rect = _player1Slot.transform as RectTransform;
            if (_player3Rect == null && _player3Slot != null)
                _player3Rect = _player3Slot.transform as RectTransform;

            if (_player1Rect == null || _player3Rect == null)
            {
                _oneVsOneLayoutActive = oneVsOne;
                return;
            }

            if (oneVsOne)
            {
                if (!_oneVsOnePositionsInitialized)
                {
                    _player1AnchorMin = _player1Rect.anchorMin;
                    _player1AnchorMax = _player1Rect.anchorMax;
                    _player3AnchorMin = _player3Rect.anchorMin;
                    _player3AnchorMax = _player3Rect.anchorMax;
                    _oneVsOnePositionsInitialized = true;
                }

                CenterSlotHorizontally(_player1Rect);
                CenterSlotHorizontally(_player3Rect);
            }
            else
            {
                _player1Rect.anchorMin = _player1AnchorMin;
                _player1Rect.anchorMax = _player1AnchorMax;
                _player3Rect.anchorMin = _player3AnchorMin;
                _player3Rect.anchorMax = _player3AnchorMax;
            }

            _oneVsOneLayoutActive = oneVsOne;
        }

        private static void CenterSlotHorizontally(RectTransform slot)
        {
            float width = slot.anchorMax.x - slot.anchorMin.x;
            float centeredMin = (1f - width) * 0.5f;
            slot.anchorMin = new Vector2(centeredMin, slot.anchorMin.y);
            slot.anchorMax = new Vector2(centeredMin + width, slot.anchorMax.y);
        }

        private void SetTeamText()
        {
            LobbyRoom room = PhotonRealtimeClient.LobbyCurrentRoom;
            int masterTeam = GetTeam(_masterClientPosition);
            if (masterTeam == 1)
            {
                if (_upperTeamText != null)
                {
                    string clanName = room.GetCustomProperty<string>(TeamBetaNameKey);
                    if (string.IsNullOrEmpty(clanName))
                    {
                        clanName = "Team Jouko";
                    }
                    _upperTeamText.text = clanName;
                }
                if (_lowerTeamText != null)
                {
                    string clanName = room.GetCustomProperty<string>(TeamAlphaNameKey);
                    if (string.IsNullOrEmpty(clanName))
                    {
                        clanName = "Team Kaarina";
                    }
                    _lowerTeamText.text = clanName;
                }
            }
            else
            {
                if (_upperTeamText != null)
                {
                    string clanName = room.GetCustomProperty<string>(TeamAlphaNameKey);
                    if (string.IsNullOrEmpty(clanName))
                    {
                        clanName = "Team Kaarina";
                    }
                    _upperTeamText.text = clanName;
                }
                if (_lowerTeamText != null)
                {
                    string clanName = room.GetCustomProperty<string>(TeamBetaNameKey);
                    if (string.IsNullOrEmpty(clanName))
                    {
                        clanName = "Team Jouko";
                    }
                    _lowerTeamText.text = clanName;
                }
            }
        }

        private static int GetTeam(int playerPos)
        {
            if (playerPos == PlayerPosition1 || playerPos == PlayerPosition2)
            {
                return TeamAlphaValue;
            }
            if (playerPos == PlayerPosition3 || playerPos == PlayerPosition4)
            {
                return TeamBetaValue;
            }
            return -1;
        }

        private void CheckMasterClient()
        {
            var curValue = PhotonRealtimeClient.LobbyCurrentRoom.GetPlayer(PhotonRealtimeClient.LobbyCurrentRoom.MasterClientId).GetCustomProperty(PlayerPositionKey, 0);
            _masterClientPosition = curValue;
            _isPlayerMaster = PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;
        }

        private void CheckOtherPlayer(LobbyPlayer player)
        {
            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                Debug.LogWarning($"{player.NickName}: Cannot find PlayerPositionKey.");
                return;
            }
            if (!player.HasCustomProperty(PlayerCharactersKey))
            {
                Debug.LogWarning($"{player.NickName}: Cannot find PlayerCharactersKey.");
                return;
            }
            if (!player.HasCustomProperty(PlayerStatsKey))
            {
                Debug.LogWarning($"{player.NickName}: Cannot find PlayerStatsKey.");
                return;
            }

            var playerPosition = player.GetCustomProperty(PlayerPositionKey, 0);
            int[] characters = player.GetCustomProperty(PlayerCharactersKey, new int[3]);
            bool ready = player.GetCustomProperty(PlayerReadyKey, false);

            bool isMasterClient = player.IsMasterClient;

            switch (playerPosition)
            {
                case PlayerPosition1:
                    if (!_interactablePlayerP1) break;
                    _interactablePlayerP1 = false;
                    if (_player1Slot != null)
                    {
                        _player1Slot.SetCharacters(player.UserId, player.NickName, characters, false, isMasterClient, _isPlayerMaster);
                        _player1Slot.SetReadyState(ready);
                        _player1SlotInUse = true;
                    }
                    break;
                case PlayerPosition2:
                    if (!_interactablePlayerP2) break;
                    _interactablePlayerP2 = false;
                    if (_player2Slot != null)
                    {
                        _player2Slot.SetCharacters(player.UserId, player.NickName, characters, false, isMasterClient, _isPlayerMaster);
                        _player2Slot.SetReadyState(ready);
                        _player2SlotInUse = true;
                    }
                    break;
                case PlayerPosition3:
                    if (!_interactablePlayerP3) break;
                    _interactablePlayerP3 = false;
                    if (_player3Slot != null)
                    {
                        _player3Slot.SetCharacters(player.UserId, player.NickName, characters, false, isMasterClient, _isPlayerMaster);
                        _player3Slot.SetReadyState(ready);
                        _player3SlotInUse = true;
                    }
                    break;
                case PlayerPosition4:
                    if (!_interactablePlayerP4) break;
                    _interactablePlayerP4 = false;
                    if (_player4Slot != null)
                    {
                        _player4Slot.SetCharacters(player.UserId, player.NickName, characters, false, isMasterClient, _isPlayerMaster);
                        _player4Slot.SetReadyState(ready);
                        _player4SlotInUse = true;
                    }
                    break;
            }
        }

        private void CheckLocalPlayer(LobbyPlayer player)
        {
            var playerPosition = player.GetCustomProperty(PlayerPositionKey, 0);
            bool ready = player.GetCustomProperty(PlayerReadyKey, false);

            // Master client can *only* start the game when in room as player!
            _interactableStartPlay = player.IsMasterClient && playerPosition >= PlayerPosition1 && playerPosition <= PlayerPosition4;
            int[] characters = new int[3];
            Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, playerData =>
            {
                CustomCharacterListObject[] character = playerData.SelectedCharacterIds;
                int i = 0;
                foreach (CustomCharacterListObject characterId in character)
                {
                    characters[i] = (int)characterId.CharacterID;
                    i++;
                }
            });

            bool isMasterClient = player.IsMasterClient;

            switch (playerPosition)
            {
                case PlayerPosition1:
                    if (!_interactablePlayerP1) break;
                    _interactablePlayerP1 = false;
                    if (_player1Slot != null)
                    {
                        _player1Slot.SetCharacters(player.UserId, player.NickName, characters, true, isMasterClient, _isPlayerMaster);
                        _player1Slot.SetReadyState(ready);
                        _player1SlotInUse = true;
                    }
                    break;
                case PlayerPosition2:
                    if (!_interactablePlayerP2) break;
                    _interactablePlayerP2 = false;
                    if (_player2Slot != null)
                    {
                        _player2Slot.SetCharacters(player.UserId, player.NickName, characters, true, isMasterClient, _isPlayerMaster);
                        _player2Slot.SetReadyState(ready);
                        _player2SlotInUse = true;
                    }
                    break;
                case PlayerPosition3:
                    if (!_interactablePlayerP3) break;
                    _interactablePlayerP3 = false;
                    if (_player3Slot != null)
                    {
                        _player3Slot.SetCharacters(player.UserId, player.NickName, characters, true, isMasterClient, _isPlayerMaster);
                        _player3Slot.SetReadyState(ready);
                        _player3SlotInUse = true;
                    }
                    break;
                case PlayerPosition4:
                    if (!_interactablePlayerP4) break;
                    _interactablePlayerP4 = false;
                    if (_player4Slot != null)
                    {
                        _player4Slot.SetCharacters(player.UserId, player.NickName, characters, true, isMasterClient, _isPlayerMaster);
                        _player4Slot.SetReadyState(ready);
                        _player4SlotInUse = true;
                    }
                    break;
            }
        }

        private void ResetState()
        {
            _interactablePlayerP1 = true;
            _interactablePlayerP2 = true;
            _interactablePlayerP3 = true;
            _interactablePlayerP4 = true;

            _player1SlotInUse = false;
            _player2SlotInUse = false;
            _player3SlotInUse = false;
            _player4SlotInUse = false;
        }

        private void SetFillBotToggle(bool value)
        {
            Debug.Log($"SetFillBotToggle {value}");
            this.Publish(new LobbyManager.BotFillToggleEvent(value));
        }

        protected override void ResetSlots()
        {
            ResetState();

            if (_player1Slot && !_player1SlotInUse) _player1Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player2Slot && !_player2SlotInUse) _player2Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player3Slot && !_player3SlotInUse) _player3Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player4Slot && !_player4SlotInUse) _player4Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
        }
    }
}
