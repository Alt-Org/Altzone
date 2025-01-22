using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelUpController : MonoBehaviour
{
    public GameObject LevelUpPanel;

    // Start is called before the first frame update
    void Start()
    {
        OpenPopup();
    }

    public void OpenPopup()
    {
        if (LevelUpPanel != null)
        {
            LevelUpPanel.SetActive(true);
        }
    }
}
