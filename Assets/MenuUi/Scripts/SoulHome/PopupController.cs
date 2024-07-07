using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    [SerializeField]
    private GameObject _popup;
    [SerializeField]
    private float popupWaitDelay = 3f;

    private IEnumerator runningCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        if (_popup == null) _popup = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDisable()
    {
        _popup.SetActive(false);
    } 

    public void ActivatePopUp(string popupText)
    {
        _popup.SetActive(true);

        Color tempColour = _popup.GetComponent<Image>().color;
        tempColour.a = 0.5f;
        _popup.GetComponent<Image>().color = tempColour;

        _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = popupText;

        Color tempTextColour = _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        tempTextColour.a = 1f;
        _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = tempTextColour;



        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }

            runningCoroutine = FadePopup(callback =>
            {
                if(callback == true)
                {
                    runningCoroutine = null;
                }
            });
        StartCoroutine(runningCoroutine);

    }

    private IEnumerator FadePopup(Action<bool> callback)
    {
        yield return new WaitForSeconds(popupWaitDelay); ;
        callback(false);
        Color tempColour = _popup.GetComponent<Image>().color;
        Color tempTextColour = _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        float startAlpha = tempColour.a;
        float startTextAlpha = tempTextColour.a;
        float startTime = 1f;

        for (float time = startTime; time >= 0; time -= Time.deltaTime)
        {
            tempColour.a = startAlpha *(time / startTime);
            _popup.GetComponent<Image>().color = tempColour;
            tempTextColour.a = startTextAlpha * (time / startTime);
            _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = tempTextColour;
            yield return null;
            callback(false);
        }
        _popup.SetActive(false);
        yield return null;
        callback(true);
    }
}
