using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public Button acceptButton;
    public Button cancelButton;

    private DailyQuest _dailyQuest;

    public TMP_Text debugText; 

    public GameObject popUpUiElement;

    private void Start()
    {
        acceptButton.onClick.AddListener(() =>
        {
            _dailyQuest.SetQuestActive();
            popUpUiElement.SetActive(false);
        });

        cancelButton.onClick.AddListener(() =>
        {
            popUpUiElement.SetActive(false);
        });
    }

    public void EnableDailyPopup(int taskID, DailyQuest selectedDaily)
    {
        popUpUiElement.SetActive(true);
        debugText.text = "Selected Quest Id: " + taskID.ToString();
        _dailyQuest = selectedDaily;
    }
}
