using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts
{
    public static partial class SignalBus
    {
        public delegate void ChangePopupInfo(string message);
        public static event ChangePopupInfo OnChangePopupInfo;
        public static void OnChangePopupInfoSignal(string message)
        {
            OnChangePopupInfo?.Invoke(message);
        }
    }

    public class PopupController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _popup;
        [SerializeField]
        private Color _textColour;
        [SerializeField]
        private Color _backgroundColour;
        [SerializeField]
        private float _popupWaitDelay = 3f;

        private IEnumerator _runningCoroutine = null;

        void OnEnable()
        {
            SignalBus.OnChangePopupInfo += ActivatePopUp;
        }

        private void Start()
        {
            _popup.SetActive(false);
        }

        void OnDisable()
        {
            _popup.SetActive(false);
            SignalBus.OnChangePopupInfo -= ActivatePopUp;
        }

        public void Initialize()
        {
            if (_popup is UnityEngine.Object obj)
            {
                if (!obj)
                {
                    _popup = transform.GetChild(0).gameObject;
                }
            }
            else
            {
                if (_popup == null) _popup = transform.GetChild(0).gameObject;
            }
        }

        public void ActivatePopUp(string popupText)
        {
            Initialize();
            if (!transform.parent.gameObject.activeInHierarchy) return; //Check if the parent is active, if not this probably shouldn't activate.
            _popup.SetActive(true);

            _popup.GetComponent<Image>().color = _backgroundColour;

            _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = popupText;

            _popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = _textColour;



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
