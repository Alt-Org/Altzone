using System;
using System.Collections;
using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MenuUI.Scripts
{
    public class ConfirmPopupController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private TextMeshProUGUI _acceptButtonText;
        [SerializeField] private Button _secondaryAcceptButton;
        [SerializeField] private TextMeshProUGUI _secondaryAcceptButtonText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;

        private UnityAction _acceptAction;
        private UnityAction _secondaryAction;
        private UnityAction _cancelAction;

        // Start is called before the first frame update
        void Start()
        {
            //if (_acceptButton != null) _acceptButtonText = _acceptButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            //if (_secondaryAcceptButton != null) _secondaryAcceptButtonText = _secondaryAcceptButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            //if (_cancelButton != null) _cancelButtonText = _cancelButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }

        public void OpenPopUp(PopupType type, UnityAction cancelAction, UnityAction acceptAction, UnityAction secondaryAcceptAction)
        {
            if (gameObject.activeSelf) return;

            _acceptAction = acceptAction;
            _secondaryAction = secondaryAcceptAction;
            _cancelAction = cancelAction;

            gameObject.SetActive(true);

            if (type is PopupType.Exit)
            {
                _infoText.text = "Sielunkodissa on tallentamattomia muutoksia. \n\n"
                + "Poistuaksesi muutokset pitää tallentaa tai hylätä. \n\n"
                + "Haluatko silti poistua? ";
                _cancelButton.onClick.AddListener(cancelAction);
                _acceptButton.onClick.AddListener(acceptAction);
                _acceptButtonText.text = "Tallenna Muutokset";
                _secondaryAcceptButton.onClick.AddListener(secondaryAcceptAction);
                _secondaryAcceptButtonText.text = "Palauta Muutokset";
            }
            else if (type is PopupType.EditClose)
            {
                _infoText.text = "Sielunkodissa on tallentamattomia muutoksia. \n\n"
                 + "Sulkeaksesi muokkaustilan tallentamattomat muutokset pitää tallentaa tai hylätä. \n\n"
                 + "Mitä haluat tehdä? ";
                _cancelButton.onClick.AddListener(cancelAction);
                _acceptButton.onClick.AddListener(acceptAction);
                _acceptButtonText.text = "Tallenna Muutokset";
                _secondaryAcceptButton.onClick.AddListener(secondaryAcceptAction);
                _secondaryAcceptButtonText.text = "Palauta Muutokset";
            }
        }

        public void ClosePopUp()
        {
            _cancelButton.onClick.RemoveListener(_cancelAction);
            _acceptButton.onClick.RemoveListener(_acceptAction);
            _secondaryAcceptButton.onClick.RemoveListener(_secondaryAction);
            gameObject.SetActive(false);
        }

    }
}
