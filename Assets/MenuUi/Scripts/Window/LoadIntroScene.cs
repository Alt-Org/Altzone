using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadIntroScene : MonoBehaviour
{
    public void moveToScene(int sceneID) {
        SceneManager.LoadScene(sceneID);
    }
}

