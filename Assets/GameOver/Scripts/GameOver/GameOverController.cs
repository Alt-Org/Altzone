using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace GameUi.Scripts.GameOver
{
    public class GameOverController : MonoBehaviour
    {
        private const float DefaultTimeout = 2.0f;

        [SerializeField] private GameOverView _view;
        [SerializeField] private float _timeOutDelay;

        private void OnEnable()
        {
            _view.Reset();
            if (!PhotonNetwork.InRoom)
            {
                _view.WinnerInfo1 = RichText.Yellow("Game was interrupted");
                return;
            }
            _view.WinnerInfo1 = RichText.Yellow("Checking results");
            if (_timeOutDelay == 0f)
            {
                _timeOutDelay = DefaultTimeout;
            }
            Debug.Log($"OnEnable {PhotonNetwork.CurrentRoom.GetDebugLabel()}");
            StartCoroutine(WaitForWinner());
        }

        private void OnDisable()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        private IEnumerator WaitForWinner()
        {
            yield return null;
            var timeOutTime = _timeOutDelay + Time.time;
            while (PhotonNetwork.InRoom)
            {
                if (Time.time > timeOutTime)
                {
                    _view.WinnerInfo1 = RichText.Yellow("No scores found");
                    break;
                }
                var winnerTeam = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamWinKey, -1);
                if (winnerTeam == -1)
                {
                    yield return null;
                    continue;
                }
                var blueScore = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamBlueScoreKey, 0);
                var redScore = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamRedScoreKey, 0);
                if (winnerTeam == PhotonBattle.TeamBlueValue)
                {
                    _view.WinnerInfo1 = RichText.Blue("Team BLUE");
                    _view.WinnerInfo2 = $"{blueScore} - {redScore}";
                }
                else if (winnerTeam == PhotonBattle.TeamRedValue)
                {
                    _view.WinnerInfo1 = RichText.Red("Team RED");
                    _view.WinnerInfo2 = $"{redScore} - {blueScore}";
                }
                else
                {
                    _view.WinnerInfo1 = RichText.Yellow("DRAW!");
                    _view.WinnerInfo2 = string.Empty;
                }
                break;
            }
            _view.ContinueButton.interactable = true;
        }
    }
}