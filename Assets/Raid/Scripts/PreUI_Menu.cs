using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PreUI_Menu : MonoBehaviour
{
    public void StartRaid()
    {
        SceneManager.LoadScene("te-test-raid-demo");
    }
}
