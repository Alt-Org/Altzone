using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class QuickMButton : MonoBehaviour
{
    [SerializeField]
    private GameObject guickMPanel;
    [SerializeField]
    private Button _button;
    private bool isGuickMOpen = false;

    void Start()
    {
        guickMPanel.SetActive(false);
        _button.onClick.AddListener(ToggleGuickM);
    }

    public void ToggleGuickM()
    {
        isGuickMOpen = !isGuickMOpen;

        guickMPanel.SetActive(isGuickMOpen);
    }
}
