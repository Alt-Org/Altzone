using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;

namespace GameOver.Scripts.GameOver
{
    public class GameOverController : MonoBehaviour
    {
        private const float DefaultTimeout = 2.0f;

        [SerializeField] private GameOverView _view;
        [SerializeField] private WindowDef _gameWindow;
        [SerializeField] private float _timeOutDelay;
        [SerializeField] private bool _isDebugKeepRoomOpen;

        private void OnEnable()
        {
            _view.Reset();
            if (!PhotonNetwork.InRoom)
            {
                _view.WinnerInfo1 = RichText.Yellow("Game was interrupted");
                _view.ContinueButton.interactable = true;
                return;
            }
            // We disable scene sync in order to prevent Photon sending scene load events to other clients because this room is finished now.
            // - PhotonLobby should set it again if/when needed.
            PhotonNetwork.AutomaticallySyncScene = false;

            // Fix room state
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.IsOpen)
            {
                PhotonLobby.CloseRoom();
            }
            _view.WinnerInfo1 = RichText.Yellow("Checking results");
            if (_timeOutDelay == 0f)
            {
                _timeOutDelay = DefaultTimeout;
            }
            _view.RestartButton.onClick.AddListener(RestartButtonClick);
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
            _view.EnableButtons();
            if (_isDebugKeepRoomOpen && Application.platform.ToString().ToLower().EndsWith("editor"))
            {
                // Do not leave room in Editor in order to be able see room state for debugging.
                yield break;
            }
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        private void RestartButtonClick()
        {
            Debug.Log($"RestartButtonClick {_gameWindow}");
            WindowManager.Get().ShowWindow(_gameWindow);
        }
    }
}