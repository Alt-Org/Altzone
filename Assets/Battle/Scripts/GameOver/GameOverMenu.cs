using System.Collections;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
using Battle.Scripts.Room;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField] private TeamScore[] _scores;
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
            _scores = TeamScore.AllocateTeamScores();
            foreach (var score in _scores)
            {
                var key = $"T{score._teamNumber}";
                var value = room.GetCustomProperty(key, -1);
                if (value > 0)
                {
                    score._wallCollisionCount = value;
                }
            }
            _team0.text = _scores[0]._wallCollisionCount > 0 ? $"Team {_scores[0]._teamNumber}\r\n{_scores[0]._wallCollisionCount}" : "No\r\nscore";
            _team1.text = _scores[1]._wallCollisionCount > 0 ? $"Team {_scores[1]._teamNumber}\r\n{_scores[1]._wallCollisionCount}" : "No\r\nscore";
            PhotonLobby.leaveRoom();
            yield return null;
            WindowManager.Get().Unwind(_mainMenuWindow);
        }
    }
}