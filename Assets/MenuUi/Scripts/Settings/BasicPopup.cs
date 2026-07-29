using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicPopup : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private List<Button> _toggleButtons = new List<Button>();

    void Start()
    {
        foreach (var button in _toggleButtons) button.onClick.AddListener(() => _content.SetActive(!_content.activeSelf));

        _content.SetActive(false);
    }
}
