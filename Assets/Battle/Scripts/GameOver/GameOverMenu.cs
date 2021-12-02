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
        [Header("Settings"), SerializeField] private UnitySceneName mainScene;
        [SerializeField] private float waitForGracefulExit;

        [Header("Scores"), SerializeField] private Text team0;
        [SerializeField] private Text team1;
        [SerializeField] private TeamScore[] scores;

        private bool isExiting;
        private float timeToExitForcefully;

        private void Start()
        {
            scores = TeamScore.AllocateTeamScores();
            if (!PhotonNetwork.InRoom)
            {
                return;
            }
            var room = PhotonNetwork.CurrentRoom;
            foreach (var score in scores)
            {
                var key = $"T{score._teamNumber}";
                var value = room.GetCustomProperty(key, -1);
                if (value > 0)
                {
                    score._wallCollisionCount = value;
                }
            }
            team0.text = scores[0]._wallCollisionCount > 0 ? $"Team {scores[0]._teamNumber}\r\n{scores[0]._wallCollisionCount}" : "No\r\nscore";
            team1.text = scores[1]._wallCollisionCount > 0 ? $"Team {scores[1]._teamNumber}\r\n{scores[1]._wallCollisionCount}" : "No\r\nscore";
            PhotonLobby.leaveRoom();
        }

        private void Update()
        {
            if (isExiting)
            {
                GotoMenu();
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.NickName}");
                GotoMenu();
            }
        }

        private void GotoMenu()
        {
            // This is not perfect but will do for now!
            if (PhotonNetwork.InRoom)
            {
                if (isExiting)
                {
                    if (Time.time > timeToExitForcefully)
                    {
                        PhotonNetwork.Disconnect();
                    }
                }
                else
                {
                    isExiting = true;
                    timeToExitForcefully = Time.time + waitForGracefulExit;
                    PhotonLobby.leaveRoom();
                    return;
                }
            }
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected || state == ClientState.ConnectedToMasterServer)
            {
                Debug.Log($"LoadScene {PhotonNetwork.NetworkClientState} {mainScene.sceneName}");
                SceneManager.LoadScene(mainScene.sceneName);
                enabled = false;
            }
        }
    }
}