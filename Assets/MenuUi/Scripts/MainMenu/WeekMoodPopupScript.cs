using System;
using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private GameObject _popupPrefab;
    enum Mood
    {
        Blank,
        Love,
        Playful,
        Joy,
        Sad,
        Angry
    }

    List<string> moodList = new List<string> { "", "", "", "", "", "", "" };

    // Start is called before the first frame update
    void Start()
    {
        _popupPrefab.SetActive(true);
    }

    public void ClosePopup()
    {
        _popupPrefab.SetActive(false);
    }

    public void SaveMoodData()
    {
        GameObject clickedObject = EventSystem.current.currentSelectedGameObject;

        string objectName = clickedObject.name;

        moodList[6] = moodList[5];
        moodList[5] = moodList[4];
        moodList[4] = moodList[3];
        moodList[3] = moodList[2];
        moodList[2] = moodList[1];
        moodList[1] = moodList[0];

        switch (objectName)
        {
            case "Love":
                moodList[0] = objectName;
                break;
            case "Playful":
                moodList[0] = objectName;
                break;
            case "Joy":
                moodList[0] = objectName;
                break;
            case "Sad":
                moodList[0] = objectName;
                break;
            case "Angry":
                moodList[0] = objectName;
                break;
        }
        Debug.Log(objectName);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
