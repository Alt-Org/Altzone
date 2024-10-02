using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts
{
    public class PopupController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _popup;
        [SerializeField]
        private float _popupWaitDelay = 3f;

        private IEnumerator _runningCoroutine = null;

        void OnDisable()
        {
            _popup.SetActive(false);
        }

        public void Initialize()
        {
            if (_popup is UnityEngine.Object obj)
            {
                if (!obj)
                {
                    _popup = gameObject;
                }
            }
            else
            {
                if (_popup == null) _popup = gameObject;
            }
        }

        public void ActivatePopUp(string popupText)
        {
            Initialize();
            _popup.SetActive(true);

            Color tempColour = _popup.GetComponent<Image>().color;
            tempColour.a = 0.5f;
            _popup.GetComponent<Image>().color = tempColour;

            _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = popupText;

            Color tempTextColour = _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
            tempTextColour.a = 1f;
            _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = tempTextColour;



            if (_runningCoroutine != null)
            {
                StopCoroutine(_runningCoroutine);
                _runningCoroutine = null;
            }

            _runningCoroutine = FadePopup(callback =>
            {
                if (callback == true)
                {
                    _runningCoroutine = null;
                }
            });
            StartCoroutine(_runningCoroutine);

        }

        private IEnumerator FadePopup(Action<bool> callback)
        {
            yield return new WaitForSeconds(_popupWaitDelay); ;
            callback(false);
            Color tempColour = _popup.GetComponent<Image>().color;
            Color tempTextColour = _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
            float startAlpha = tempColour.a;
            float startTextAlpha = tempTextColour.a;
            float startTime = 1f;

            for (float time = startTime; time >= 0; time -= Time.deltaTime)
            {
                tempColour.a = startAlpha * (time / startTime);
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
}
