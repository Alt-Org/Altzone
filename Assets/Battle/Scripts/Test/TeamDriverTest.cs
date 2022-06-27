using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Game;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class TeamDriverTest : MonoBehaviour
    {
        [Serializable]
        internal class TeamSettings
        {
            public MonoBehaviour _player1;
            public MonoBehaviour _player2;
        }

        [Header("Autostart"), SerializeField] private bool _isAutoStartBall;
        [Range(0, 4), SerializeField] private int _roomPlayersOverride;

        [Header("Settings"), SerializeField] private bool _isShowCountdown;

        [Header("Live Data"), SerializeField] private TeamSettings _teamBlue;
        [SerializeField] private TeamSettings _teamRed;

        private void OnEnable()
        {
            Debug.Log($"");
            this.Subscribe<TeamCreated>(OnTeamCreated);
            this.Subscribe<TeamUpdated>(OnTeamUpdated);
            this.Subscribe<TeamBroken>(OnTeamBroken);
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
            StartCoroutine(ConfigureRoom(_roomPlayersOverride));
        }

        private static IEnumerator ConfigureRoom(int roomPlayersCount)
        {
            yield return new WaitUntil(() => PhotonNetwork.InRoom);
            var room = PhotonNetwork.CurrentRoom;
            if (roomPlayersCount > 0)
            {
                PhotonBattle.SetDebugRoomProperties(room, roomPlayersCount);
            }
        }

        private void OnDisable()
        {
            Debug.Log($"");
            this.Unsubscribe();
        }

        private void OnTeamCreated(TeamCreated data)
        {
            var team = data.BattleTeam;
            Debug.Log($"team {team} first {team.FirstPlayer.NickName} second {team.SecondPlayer?.NickName}");
            UpdateTeam(team);
        }

        private void OnTeamUpdated(TeamUpdated data)
        {
            var team = data.BattleTeam;
            Debug.Log($"team {team} first {team.FirstPlayer.NickName} second {team.SecondPlayer.NickName}");
            UpdateTeam(team);
        }

        private void OnTeamBroken(TeamBroken data)
        {
            var team = data.BattleTeam;
            Debug.Log($"team {team} first {team.FirstPlayer.NickName} LEFT {data.PlayerWhoLeft.NickName}");
            UpdateTeam(team);
        }

        private void UpdateTeam(BattleTeam team)
        {
            if (team.FirstPlayer.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                _teamBlue._player1 = team.FirstPlayer as MonoBehaviour;
                _teamBlue._player2 = team.SecondPlayer as MonoBehaviour;
            }
            else if (team.FirstPlayer.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _teamRed._player1 = team.FirstPlayer as MonoBehaviour;
                _teamRed._player2 = team.SecondPlayer as MonoBehaviour;
            }
        }
        
        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            Debug.Log($"TeamsAreReadyForGameplay {data.TeamBlue} vs {data.TeamRed?.ToString() ?? "null"} " +
                      $"IsMasterClient {PhotonNetwork.IsMasterClient}");
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            PhotonLobby.CloseRoom();
            if (_isAutoStartBall)
            {
                var startTheBallTest = FindObjectOfType<StartTheBallTest>();
                if (startTheBallTest != null)
                {
                    ScoreFlashNet.Push("AUTOSTART");
                    startTheBallTest.StartBallFirstTime();
                }
            }
            if (!_isShowCountdown)
            {
                return;
            }
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            var countdownDelay = variables._roomStartDelay;
            StartCoroutine(SimulateCountdown(countdownDelay));
        }

        private static IEnumerator SimulateCountdown(int countdownDelay)
        {
            var delay = new WaitForSeconds(1f);
            while (--countdownDelay >= 0)
            {
                yield return delay;
                ScoreFlashNet.Push(countdownDelay.ToString());
            }
        }
    }
}