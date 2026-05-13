using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownOpenCloseWatcher : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Dropdown _dropdown;

    public Action<bool> OnDropdownOpenChanged;

    private bool _isOpen;

    private void Awake()
    {
        if (_dropdown == null)
            _dropdown = GetComponent<TMP_Dropdown>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(CheckOpenStateNextFrame());
    }

    private void Update()
    {
        bool actuallyOpen = IsDropdownListOpen();

        if (_isOpen != actuallyOpen)
        {
            _isOpen = actuallyOpen;
            OnDropdownOpenChanged?.Invoke(_isOpen);
        }
    }

    public void Close()
    {
        _isOpen = false;
        OnDropdownOpenChanged?.Invoke(false);
    }

    private System.Collections.IEnumerator CheckOpenStateNextFrame()
    {
        yield return null;

        bool actuallyOpen = IsDropdownListOpen();

        if (_isOpen != actuallyOpen)
        {
            _isOpen = actuallyOpen;
            OnDropdownOpenChanged?.Invoke(_isOpen);
        }
    }

    private bool IsDropdownListOpen()
    {
        if (_dropdown == null) return false;

        Transform dropdownList = _dropdown.transform.Find("Dropdown List");
        return dropdownList != null && dropdownList.gameObject.activeInHierarchy;
    }

    private void OnDisable()
    {
        Close();
    }
}
