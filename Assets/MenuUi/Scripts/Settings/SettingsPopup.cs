using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [SerializeField] private Button[] _closeButtons;   // esim. se n�kym�t�n taustabuttoni
    [SerializeField] private SwipeBlockerPopupHandler _blockerHandler;

    private void Start()
    {
        foreach (Button button in _closeButtons)
        {
            button.onClick.AddListener(() => StartCoroutine(CloseWithDelay()));
        }
    }

    private void OnDisable()
    {
        gameObject.SetActive(false);

        if(_blockerHandler) _blockerHandler.ClosePopup(gameObject);
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);

        if (_blockerHandler) _blockerHandler.OpenPopup(gameObject);
    }

    public IEnumerator CloseWithDelay()
    {
        if (!isActiveAndEnabled) yield break;
        yield return new WaitForSecondsRealtime(0.3f);
        ClosePopup();
    }

    public void ClosePopup(bool invokeButtons = true)
    {
        if (!isActiveAndEnabled) return;
        gameObject.SetActive(false);

        if (_blockerHandler) _blockerHandler.ClosePopup(gameObject);

        if (!invokeButtons) return;

        //This is here because some buttons shut down more than one tab
        foreach (Button objects in _closeButtons)
        {
            objects.onClick.Invoke();
        }

    }
}
