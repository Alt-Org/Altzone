using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeleteMessage : MonoBehaviour
{
    public GameObject deletePanelPrefab;
    private GameObject deletePanelInstance;

    public Button messageButton;

    private bool deletePanelVisible = false;

    private void Start()
    {
        //messageButton = GetComponentInChildren<Button>();

        if (messageButton != null)
        {
            messageButton.onClick.AddListener(OnButtonClick);
        }
   
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if(!deletePanelVisible)
                {
                    ShowDeletePanel();
                }
                else
                {
                    HideDeletePanel();
                }
            }
        }
    }

    public void ShowDeletePanel()
    {
        deletePanelInstance = Instantiate(deletePanelPrefab, transform.parent);

        deletePanelInstance.SetActive(true);
        deletePanelVisible = true;
    }

    public void HideDeletePanel()
    {
        if(deletePanelInstance != null)
        {
            deletePanelInstance.SetActive(false);
            deletePanelVisible = false;
        }
    }

    private void OnButtonClick()
    {
        Debug.Log("Delete button presed!");
    }
}
