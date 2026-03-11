using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanelButtonScaler : MonoBehaviour
{
    [SerializeField] private List<GameObject> BottomButtons = new List<GameObject>();

    private Vector3 buttonScale = new Vector3();

    void Start()
    {
        buttonScale = BottomButtons[2].transform.localScale;
        BottomButtons[2].transform.localScale = buttonScale * 1.2f;
    }


    public void ScaleIcons(int index)
    {
        foreach (GameObject button in BottomButtons)
        {
            button.transform.localScale = buttonScale;
        }

        BottomButtons[index].transform.localScale = buttonScale * 1.2f;

    }
}
