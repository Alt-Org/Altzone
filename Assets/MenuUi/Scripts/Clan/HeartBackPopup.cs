using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Window;

public class HeartBackPopup : MonoBehaviour
{
    public GameObject popupPanel;
    public Button backButton;
    public Button sureButton;
    public Button closeButton;

    public ClanHeartColorChanger heartColorChanger;

    void Start()
    {
        popupPanel.SetActive(false);

        backButton.onClick.AddListener(OnBackButtonClick);
        sureButton.onClick.AddListener(OnSureButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }

    void OnBackButtonClick()
    {

        if (!heartColorChanger.IsAnyPieceChanged())
        {
            Debug.Log("No changes, Going Back.");
            StartCoroutine(GoBack());
        }
        else
        {
            popupPanel.SetActive(true);
        }
    }

    void OnSureButtonClick()
    {
        Debug.Log("Continue without saving.");
        popupPanel.SetActive(false);
        StartCoroutine(GoBack());
    }

    void OnCloseButtonClick()
    {
        popupPanel.SetActive(false);
    }

    private IEnumerator GoBack()
    {
        yield return null;
        WindowManager.Get().GoBack();
    }
}
