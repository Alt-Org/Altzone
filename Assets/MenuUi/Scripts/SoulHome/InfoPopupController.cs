using System;
using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts
{
    public enum InfoLevel
    {
        Info = 0,
        Warning = 1,
        Error = 2
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
        private Color _backgroundWarningColour;
        [SerializeField]
        private Color _backgroundErrorColour;
        [SerializeField]
        private float _popupWaitDelay = 3f;

        private IEnumerator _runningCoroutine = null;

        void OnEnable()
        {
            OverlayPanelCheck.OnChangePopupInfo += ActivatePopUp;
        }

        private void Start()
        {
            _popup.SetActive(false);
        }

        void OnDisable()
        {
            _popup.SetActive(false);
            OverlayPanelCheck.OnChangePopupInfo -= ActivatePopUp;
        }

        public void Initialize()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true); //Make sure that this object is always active when called.
        }

        public void ActivatePopUp(InfoLevel level, string popupText)
        {
            Initialize();
            if (!transform.parent.gameObject.activeInHierarchy) return; //Check if the parent is active, if not this probably shouldn't activate.
            _popup.SetActive(true);

            switch (level)
            {
                case InfoLevel.Info:
                    ActivateInfoPopUp(popupText);
                    break;
                case InfoLevel.Warning:
                    ActivateWarningPopUp(popupText);
                    break;
                case InfoLevel.Error:
                    ActivateErrorPopUp(popupText);
                    break;
                default:
                    ActivateInfoPopUp(popupText);
                    break;
            }

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


        public void ActivateInfoPopUp(string popupText)
        {
            _background.color = _backgroundInfoColour;
            _textField.text = popupText;
            _textField.color = _textColour;
        }

        public void ActivateWarningPopUp(string popupText)
        {
            _background.color = _backgroundWarningColour;
            _textField.text = popupText;
            _textField.color = _textColour;
        }

        public void ActivateErrorPopUp(string popupText)
        {
            _background.color = _backgroundErrorColour;
            _textField.text = popupText;
            _textField.color = _textColour;
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
