using Battle.Scripts.Player;
using Battle.Scripts.Room;
using Battle.Scripts.Test;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.UI
{
    /// <summary>
    /// Simple temporary UI for debugging.
    /// </summary>
    public class DebugCanvasListener : MonoBehaviour
    {
        private const string ScoreFormat = "<color={4}>{0}({1})</color> h={2} w={3}";

        public GameObject roomStartPanel;
        public Text titleText;
        public GameObject scorePanel;
        public Text leftText;
        public Text rightText;
        public CountdownText countdown;

        private string _teamNameHome;
        private string _teamNameVisitor;
        private string _teamColorHome;
        private string _teamColorVisitor;

        private void OnEnable()
        {
            roomStartPanel.SetActive(false);
            scorePanel.SetActive(false);
            leftText.text = "";
            rightText.text = "";
            this.Subscribe<ScoreManager.TeamNameEvent>(OnTeamNameEvent);
            this.Subscribe<ScoreManager.TeamScoreEvent>(OnTeamScoreEvent);
            this.Subscribe<GameStartPlayingTest.CountdownEvent>(OnCountdownEvent);
        }

        private void OnDisable()
        {
            leftText.text = "";
            rightText.text = "";
            this.Unsubscribe();
        }

        private void OnCountdownEvent(GameStartPlayingTest.CountdownEvent data)
        {
            Debug.Log($"OnCountdownEvent {data}");
            if (data.maxCountdownValue == data.curCountdownValue)
            {
                roomStartPanel.SetActive(true);
                titleText.text = "Wait for game start:";
            }
            countdown.setCountdownValue(data.curCountdownValue);
            if (data.curCountdownValue <= 0)
            {
                this.ExecuteAsCoroutine(new WaitForSeconds(0.67f), () =>
                {
                    roomStartPanel.SetActive(false);
                    scorePanel.SetActive(true);
                });
            }
        }

        private void OnTeamNameEvent(ScoreManager.TeamNameEvent data)
        {
            _teamNameHome = data.HomeTeamName;
            _teamNameVisitor = data.VisitorTeamName;
            _teamColorHome = "yellow";
            _teamColorVisitor = "white";
        }

        private void OnTeamScoreEvent(ScoreManager.TeamScoreEvent data)
        {
            Debug.Log($"OnTeamScoreEvent {data} local {PlayerActivator.LocalTeamNumber} home {PlayerActivator.HomeTeamNumber}");
            var score = data.Score;
            // Local or remote player - left or right side
            var isLocalTeam = score._teamNumber == PlayerActivator.LocalTeamNumber;
            // Master client team - team name and color
            var isHomeTeam = score._teamNumber == PlayerActivator.HomeTeamNumber;
            var teamName = isHomeTeam ? _teamNameHome : _teamNameVisitor;
            var teamColor = isHomeTeam ? _teamColorHome : _teamColorVisitor;
            var text = isLocalTeam ? leftText : rightText;
            text.text = string.Format(ScoreFormat, teamName, score._teamNumber, score._headCollisionCount, score._wallCollisionCount, teamColor);
        }
    }
}