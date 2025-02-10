using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelActivnessController : MonoBehaviour
{
    [SerializeField] private GameObject _Panel;

    public void HandlePanel()
    {
        _Panel.SetActive(!_Panel.activeSelf);
    }
    public void OpenPanel() => _Panel.SetActive(true);

    public void ClosePanel() => _Panel.SetActive(false);
}
