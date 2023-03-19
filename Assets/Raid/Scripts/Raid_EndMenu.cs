using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Raid_EndMenu : MonoBehaviour
{
    public void ReturnToLobby()
    {
        SceneManager.LoadScene("20-Lobby");
    }
}
