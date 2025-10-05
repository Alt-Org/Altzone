using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using MenuUI.Scripts.SoulHome;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MenuUI.Scripts
{
    public class ConfirmPopupController : MonoBehaviour
    {
        [SerializeField] private TextLanguageSelectorCaller _infoText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private TextLanguageSelectorCaller _acceptButtonText;
        [SerializeField] private Button _secondaryAcceptButton;
        [SerializeField] private TextLanguageSelectorCaller _secondaryAcceptButtonText;
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

            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                if (type is PopupType.Exit)
                {
                    _infoText.SetText("Sielunkodissa on tallentamattomia muutoksia. \n\n"
                    + "Poistuaksesi muutokset pitää tallentaa tai hylätä. \n\n"
                    + "Haluatko silti poistua? ");
                    _cancelButton.onClick.AddListener(cancelAction);
                    _acceptButton.onClick.AddListener(acceptAction);
                    _acceptButtonText.SetText("Tallenna Muutokset");
                    _secondaryAcceptButton.onClick.AddListener(secondaryAcceptAction);
                    _secondaryAcceptButtonText.SetText("Palauta Muutokset");
                }
                else if (type is PopupType.EditClose)
                {
                    _infoText.SetText("Sielunkodissa on tallentamattomia muutoksia. \n\n"
                     + "Sulkeaksesi muokkaustilan tallentamattomat muutokset pitää tallentaa tai hylätä. \n\n"
                     + "Mitä haluat tehdä? ");
                    _cancelButton.onClick.AddListener(cancelAction);
                    _acceptButton.onClick.AddListener(acceptAction);
                    _acceptButtonText.SetText("Tallenna Muutokset");
                    _secondaryAcceptButton.onClick.AddListener(secondaryAcceptAction);
                    _secondaryAcceptButtonText.SetText("Palauta Muutokset");
                }
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                if (type is PopupType.Exit)
                {
                    _infoText.SetText("Castle has some unsaved changes. \n\n"
                    + "These changes must be saved or discarded. \n\n"
                    + "Do you still want to leave? ");
                    _cancelButton.onClick.AddListener(cancelAction);
                    _acceptButton.onClick.AddListener(acceptAction);
                    _acceptButtonText.SetText("Save Changes");
                    _secondaryAcceptButton.onClick.AddListener(secondaryAcceptAction);
                    _secondaryAcceptButtonText.SetText("Revert Changes");
                }
                else if (type is PopupType.EditClose)
                {
                    _infoText.SetText("Castle has some unsaved changes. \n\n"
                     + "To close the modification mode you need to save or discarned. \n\n"
                     + "What do you want to do. ");
                    _cancelButton.onClick.AddListener(cancelAction);
                    _acceptButton.onClick.AddListener(acceptAction);
                    _acceptButtonText.SetText("Save Changes");
                    _secondaryAcceptButton.onClick.AddListener(secondaryAcceptAction);
                    _secondaryAcceptButtonText.SetText("Revert Changes");
                }
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
