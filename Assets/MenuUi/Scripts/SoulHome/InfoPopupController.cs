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

    public class InfoPopupController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _popup;
        [SerializeField]
        private Image _background;
        [SerializeField]
        private TMP_Text _textField;
        [SerializeField]
        private Color _textColour;
        [SerializeField]
        private Color _backgroundInfoColour;
        [SerializeField]
        private Color _backgroundErrorColour;
        [SerializeField]
        private float _popupWaitDelay = 3f;

        private IEnumerator _runningCoroutine = null;

        void OnEnable()
        {
            SignalBus.OnChangePopupInfo += ActivateInfoPopUp;
        }

        private void Start()
        {
            _popup.SetActive(false);
        }

        void OnDisable()
        {
            _popup.SetActive(false);
            SignalBus.OnChangePopupInfo -= ActivateInfoPopUp;
        }

        public void Initialize()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true); //Make sure that this object is always active when called.
        }
        
        public void ActivateInfoPopUp(string popupText)
        {
            Initialize();
            if (!transform.parent.gameObject.activeInHierarchy) return; //Check if the parent is active, if not this probably shouldn't activate.
            _popup.SetActive(true);

            _background.color = _backgroundInfoColour;

            _textField.text = popupText;

            _textField.color = _textColour;



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

        public void ActivateErrorPopUp(string popupText)
        {
            Initialize();
            if (!transform.parent.gameObject.activeInHierarchy) return; //Check if the parent is active, if not this probably shouldn't activate.
            _popup.SetActive(true);

            _background.color = _backgroundInfoColour;

            _textField.text = popupText;

            _textField.color = _textColour;



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
            Color tempColour = _background.color;
            Color tempTextColour = _textField.color;
            float startAlpha = tempColour.a;
            float startTextAlpha = tempTextColour.a;
            float startTime = 1f;

            for (float time = startTime; time >= 0; time -= Time.deltaTime)
            {
                tempColour.a = startAlpha * (time / startTime);
                _background.color = tempColour;
                tempTextColour.a = startTextAlpha * (time / startTime);
                _textField.color = tempTextColour;
                yield return null;
                callback(false);
            }
            _popup.SetActive(false);
            yield return null;
            callback(true);
        }
    }
}
