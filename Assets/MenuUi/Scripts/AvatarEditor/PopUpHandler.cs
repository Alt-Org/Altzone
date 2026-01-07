using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace MenuUi.Scripts.AvatarEditor
{
    public class PopUpHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _character;
        [SerializeField] private GameObject _popUp;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _confirmButton;

        private GameObject _characterCopy;
        private RectTransform _characterCopyRect;
        [SerializeField] private RectTransform _characterParent;

        private void Start()
        {
            _cancelButton.onClick.AddListener(() => HidePopUp());
        }

        public void AddConfirmButtonListener(Action buttonFunction)
        {
            _confirmButton.onClick.AddListener(() =>
            {
                buttonFunction.Invoke();
                HidePopUp();
            });
        }

        public void ShowPopUp()
        {
            _characterCopy = Instantiate(_character, _characterParent);
            _characterCopyRect = _characterCopy.GetComponent<RectTransform>();
            _characterCopyRect.anchorMin = new Vector2(0.15f, 0f);
            _characterCopyRect.anchorMax = new Vector2(0.6f, 0.85f);

            _popUp.SetActive(true);
        }

        public void HidePopUp()
        {
            if (_characterCopy != null)
            {
                Destroy(_characterCopy);
            }

            _popUp.SetActive(false);
        }
    }
}
