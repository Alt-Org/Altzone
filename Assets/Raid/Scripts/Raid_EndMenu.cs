using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Raid_EndMenu : MonoBehaviour
{
    public void ReturnToLobby()
    {
        SceneManager.LoadScene("10-MenuUI");
    }
    //TODO: This is used for testing / debugging only, remove when releasing demo
    public void Restart() 
    {
        SceneManager.LoadScene("41-QuantumRaid");
    }
}
