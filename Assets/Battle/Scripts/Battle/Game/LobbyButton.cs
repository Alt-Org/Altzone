using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyButton : MonoBehaviour
{
    //[SerializeField] SceneDef Lobby;

    public void ToLobby()
    {
        SceneManager.LoadScene("20-Lobby");       //Lobby.SceneName
    }
}
