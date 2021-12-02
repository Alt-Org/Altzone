using Altzone.Scripts.Battle;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battle.Scripts.Room
{
    /// <summary>
    /// Helper class to load game over screen.
    /// </summary>
    /// <remarks>
    /// Do not close room here because loading new level during room close leads to errors from Photon!
    /// </remarks>
    public class GameOver : MonoBehaviourPunCallbacks
    {
        [SerializeField] private UnitySceneName gameOverScene;

        private bool isLoading;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"Escape {PhotonWrapper.NetworkClientState}");
                gotoMainMenu();
                enabled = false;
            }
        }

        private void OnApplicationQuit()
        {
            isLoading = true; // Fake to prevent loading, probably we are over cautious here!?
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
            if (PhotonNetwork.IsMasterClient && PhotonBattle.IsRealPlayer(otherPlayer))
            {
                gotoMainMenu();
                enabled = false;
            }
        }

        private void gotoMainMenu()
        {
            if (!isLoading)
            {
                isLoading = true;
                SceneManager.LoadScene(gameOverScene.sceneName);
            }
        }
    }
}