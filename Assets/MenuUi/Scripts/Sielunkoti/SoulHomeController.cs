using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;
using TMPro;
using MenuUi.Scripts.Window;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome
{
    public class SoulHomeController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _roomName;
        [SerializeField]
        private TowerController _soulHomeTower;
        [SerializeField]
        private MainScreenController _mainScreen;
        [SerializeField]
        private GameObject _confirmPopup;
        [SerializeField]
        private PopupController _infoPopup;

        private bool _confirmPopupOpen = false;
        private bool _exitPending = false;

        public bool ExitPending { get => _exitPending;}
        public bool ConfirmPopupOpen { get => _confirmPopupOpen;}

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetRoomName(GameObject room)
        {
            if (room != null)
            {
                _roomName.SetActive(true);
                string roomName = room.GetComponent<RoomData>().RoomInfo.Id.ToString();
                _roomName.GetComponent<TextMeshProUGUI>().text = "Huone " + roomName;
            }
            else _roomName.SetActive(false);
        }

        public void ExitSoulHome()
        {
            if (!_exitPending)
            if(_soulHomeTower.ChangedFurnitureList.Count > 0)
            {
                _exitPending = true;
                _confirmPopup.SetActive(true);
                _confirmPopup.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Sielunkodissa on tallentamattomia muutoksia. \n\n"
                + "Poistumalla hylkäät tallentamattomat muutokset. \n\n"
                + "Haluatko silti poistua? ";
                _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => ConfirmExit(false));
                _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.AddListener(() => ConfirmExit(true));
                _confirmPopup.transform.Find("AcceptButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sulje sielunkoti";
                }
            else
            WindowManager.Get().GoBack();
        }

        public void ConfirmExit(bool confirm)
        {
            _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveListener(() => ConfirmExit(false));
            _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.RemoveListener(() => ConfirmExit(true));
            if (confirm)
            {
                _mainScreen.ResetChanges();
                _exitPending=false;
                _confirmPopup.SetActive(false);
                WindowManager.Get().GoBack();
            }
            else
            {
                _exitPending=false;
                _confirmPopup.SetActive(false);
            }
        }

        public void EditModeToggle()
        {
            if (_exitPending || _confirmPopupOpen) return;
            if (!_soulHomeTower.EditingMode) _soulHomeTower.ToggleEdit();
            else
            {
                if (_soulHomeTower.ChangedFurnitureList.Count > 0)
                {
                    _confirmPopupOpen = true;
                    _confirmPopup.SetActive(true);
                    _confirmPopup.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Sielunkodissa on tallentamattomia muutoksia. \n\n"
                    + "Sulkemalla muokkaustilan hylkäät tallentamattomat muutokset. \n\n"
                    + "Haluatko silti sulkea muokkaustilan? ";
                    _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => ConfirmEditClose(false));
                    _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.AddListener(() => ConfirmEditClose(true));
                    _confirmPopup.transform.Find("AcceptButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sulje muokkaustila";
                }
                else
                    _soulHomeTower.ToggleEdit();
            }
        }
        public void ConfirmEditClose(bool confirm)
        {
            _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveListener(() => ConfirmEditClose(false));
            _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.RemoveListener(() => ConfirmEditClose(true));
            if (confirm)
            {
                _mainScreen.ResetChanges();
                _confirmPopupOpen = false;
                _confirmPopup.SetActive(false);
                _soulHomeTower.ToggleEdit();
            }
            else
            {
                _confirmPopupOpen = false;
                _confirmPopup.SetActive(false);
            }
        }

        public void ShowInfoPopup(string popupText)
        {
            _infoPopup.ActivatePopUp(popupText);
        }

    }
}
