using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyButton : MonoBehaviour
{
    public void ToLobby()
    {
        SceneManager.LoadScene("72-Battle-Lobby");
    }
}
