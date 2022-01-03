using Altzone.Scripts.Battle;
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
        private const string ScoreFormat = "{0}({1}) h={2} w={3}";

        public GameObject roomStartPanel;
        public Text titleText;
        public GameObject scorePanel;
        public Text leftText;
        public Text rightText;
        public CountdownText countdown;

        private string _teamBlueName;
        private string _teamRedName;

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
            _teamBlueName = data.TeamBlueName;
            _teamRedName = data.TeamRedName;
        }

        private void OnTeamScoreEvent(ScoreManager.TeamScoreEvent data)
        {
            var score = data.Score;
            var isBlueTeam = score._teamNumber == PhotonBattle.TeamBlueValue;
            var teamName = isBlueTeam ? _teamBlueName : _teamRedName;
            var text = isBlueTeam ? leftText : rightText;
            text.text = string.Format(ScoreFormat, teamName, score._teamNumber, score._headCollisionCount, score._wallCollisionCount);
        }
    }
}