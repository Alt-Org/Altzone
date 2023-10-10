using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelOpen : MonoBehaviour
{
 public GameObject panelToOpen;

    public Button button;
    public Button button2;

    private void Awake()
    {
        button = GetComponent<Button>();

        // Add a listener to the button
        button.onClick.AddListener(OpenPanel);
        button2.onClick.AddListener(ClosePanel);
    }

    private void OpenPanel()
    {
        // Set the panel to active
        panelToOpen.SetActive(true);
    }
     public void ClosePanel()
    {
        panelToOpen.SetActive(false);
    }
}
