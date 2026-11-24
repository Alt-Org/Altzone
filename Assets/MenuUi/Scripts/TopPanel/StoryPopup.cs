using System.Collections;
using System.Collections.Generic;
using Prg.Scripts.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoryPopup : MonoBehaviour
{
    [SerializeField] private Button _openStoryPopupButton1;
    [SerializeField] private Button _openStoryPopupButton2;
    [SerializeField] private Button _openStoryPopupButton3;
    [SerializeField] private GameObject _watchPopup;

    void Start()
    {
        _openStoryPopupButton1.onClick.AddListener(() => OpenStoryPopup());
        _openStoryPopupButton2.onClick.AddListener(() => OpenStoryPopup());
        _openStoryPopupButton3.onClick.AddListener(() => OpenStoryPopup());
    }

    private void OpenStoryPopup()
    {
        _watchPopup.SetActive(true);
    }
    
    
    // for closing the popup and registering inputs correctly
    private bool _closing = false;

    private void LateUpdate()
    {
        if (ClickStateHandler.GetClickState() is ClickState.Start)
        {
            if (_watchPopup.activeSelf)
            {
                if (!CheckIfPanel()) _closing = true;
            }
        }
        if (ClickStateHandler.GetClickState() is ClickState.End && _closing)
        {
            if (!CheckIfPanel()) Hide();
            _closing = false;
        }
    }

    private bool CheckIfPanel()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData data = new(EventSystem.current);
        data.position = ClickStateHandler.GetClickPosition();
        if (data.position == Vector2.negativeInfinity) return false;
        var modules = RaycasterManager.GetRaycasters();
        foreach (var module in modules)
        {
            module.Raycast(data, results);
        }
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == _watchPopup) return true;
        }
        return false;
    }

    public void Hide()
    {
        StartCoroutine(WaitMenuClose());
    }

    IEnumerator WaitMenuClose()
    {
        //Wait for 0.3 seconds
        yield return new WaitForSecondsRealtime(0.3f);

        _watchPopup.SetActive(false);
    }
}
