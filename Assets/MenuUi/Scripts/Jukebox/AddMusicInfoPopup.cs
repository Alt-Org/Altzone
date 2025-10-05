using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddMusicInfoPopup : MonoBehaviour
{
    [SerializeField] private List<Button> _closePopupButtons;
    [SerializeField] private Button _webLinkButton;
    [SerializeField] private string _webLinkText;

    void Start()
    {
        foreach (Button button in _closePopupButtons) button.onClick.AddListener(() => { gameObject.SetActive(false); });
        _webLinkButton.onClick.AddListener(() => { Application.OpenURL(_webLinkText); });
    }
}
