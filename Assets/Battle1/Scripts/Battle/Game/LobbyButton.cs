using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battle1.Scripts.Battle.Game
{
    public class LobbyButton : MonoBehaviour
    {
        //[SerializeField] SceneDef Lobby;

        public void ToLobby()
        {
            SceneManager.LoadScene("20-Lobby");       //Lobby.SceneName
        }
    }
}
