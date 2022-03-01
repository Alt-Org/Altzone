using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Battle0.Scripts.GameOver
{
    /// <summary>
    /// Utility to close this room properly before allowing to exit to main menu!
    /// </summary>
    /// <remarks>
    /// And show scores and close room.
    /// </remarks>
    public class GameOverMenu : MonoBehaviour
    {
        [Header("Scores"), SerializeField] private Text _team0;
        [SerializeField] private Text _team1;
        [SerializeField] private WindowDef _mainMenuWindow;
        [SerializeField] private float _timeOutDelay;

        private IEnumerator Start()
        {
            _team0.text = string.Empty;
            _team1.text = string.Empty;
            if (!PhotonNetwork.InRoom)
            {
                yield return null;
                WindowManager.Get().ShowWindow(_mainMenuWindow);
                yield break;
            }
            var room = PhotonNetwork.CurrentRoom;
            var timeOutTime = _timeOutDelay + Time.time;
            while (PhotonWrapper.InRoom)
            {
                if (Time.time > timeOutTime)
                {
                    _team0.text = "???";
                    _team1.text = "???";
                    break;
                }
                var winnerTeam = room.GetCustomProperty(PhotonBattle.TeamWinKey, -1);
                if (winnerTeam == -1)
                {
                    yield return null;
                    continue;
                }

                Debug.Log($"GameOver {room.GetDebugLabel()}");
                var blueName = room.GetCustomProperty(PhotonBattle.TeamBlueNameKey, string.Empty);
                var blueScore = room.GetCustomProperty(PhotonBattle.TeamBlueScoreKey, 0);
                var redName = room.GetCustomProperty(PhotonBattle.TeamRedNameKey, string.Empty);
                var redScore = room.GetCustomProperty(PhotonBattle.TeamRedScoreKey, 0);

                _team0.text = $"{blueName} {blueScore}";
                _team1.text = $"{redName} {redScore}";
                break;
            }
            PhotonLobby.LeaveRoom();
            while (PhotonWrapper.InRoom)
            {
                yield return null;
            }
            WindowManager.Get().Unwind(_mainMenuWindow);
        }
    }
}