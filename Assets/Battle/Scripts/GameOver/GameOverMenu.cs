using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.GameOver
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
            var blueName = room.GetCustomProperty(PhotonBattle.TeamBlueNameKey, string.Empty);
            var blueScore = room.GetCustomProperty(PhotonBattle.TeamBlueScoreKey, 0);
            var redName = room.GetCustomProperty(PhotonBattle.TeamRedNameKey, string.Empty);
            var redScore = room.GetCustomProperty(PhotonBattle.TeamRedScoreKey, 0);

            // We do no know which one was home team :-(
            _team0.text = $"{blueName} {blueScore}";
            _team1.text = $"{redName} {redScore}";
            PhotonLobby.LeaveRoom();
            yield return null;
            WindowManager.Get().Unwind(_mainMenuWindow);
        }
    }
}