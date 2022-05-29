using System;
using Altzone.Scripts.Battle;
using Battle.Test.Scripts.Battle.Players;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    public class TeamDriverTest : MonoBehaviour
    {
        [Serializable]
        internal class TeamSettings
        {
            public MonoBehaviour _player1;
            public MonoBehaviour _player2;
        }

        [SerializeField] private TeamSettings _teamBlue;
        [SerializeField] private TeamSettings _teamRed;
        private void OnEnable()
        {
            this.Subscribe<TeamCreated>(OnTeamCreated);
            this.Subscribe<TeamBroken>(OnTeamBroken);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnTeamCreated(TeamCreated data)
        {
            var team = data.BattleTeam;
            Debug.Log($"first {team.FirstPlayer.NickName} second {team.SecondPlayer.NickName}");
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

        private void OnTeamBroken(TeamBroken data)
        {
            Debug.Log($"player left {data.PlayerWhoLeft.NickName}");
            if (data.PlayerWhoLeft.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                _teamBlue._player1 = null;
                _teamBlue._player2 = null;
            }
            else if (data.PlayerWhoLeft.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _teamRed._player1 = null;
                _teamRed._player2 = null;
            }
        }
    }
}