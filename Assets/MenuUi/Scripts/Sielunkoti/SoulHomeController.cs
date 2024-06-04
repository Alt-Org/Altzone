using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;
using TMPro;
using MenuUi.Scripts.Window;
using UnityEngine.UI;
using AltZone.Scripts.GA;

namespace MenuUI.Scripts.SoulHome
{
    public enum PopupType
    {
        Exit,
        EditClose
    }

    public class SoulHomeController : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _clanName;
        [SerializeField]
        private TextMeshProUGUI _roomName;
        [SerializeField]
        private TowerController _soulHomeTower;
        [SerializeField]
        private MainScreenController _mainScreen;
        [SerializeField]
        private GameObject _confirmPopup;
        [SerializeField]
        private PopupController _infoPopup;
        [SerializeField]
        private Button _editButton;
        [SerializeField]
        private GameObject _editTray;
        [SerializeField]
        private GameObject _audioManager;


        private bool _confirmPopupOpen = false;
        private bool _exitPending = false;

        public bool ExitPending { get => _exitPending;}
        public bool ConfirmPopupOpen { get => _confirmPopupOpen;}

        // Start is called before the first frame update
        void Start()
        {
            if (ServerManager.Instance.Clan != null)
            {
                _clanName.text = $"Klaanin {ServerManager.Instance.Clan.name} Sielunkoti";
            }
            EditModeTrayResize();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnEnable()
        {
            if(_infoPopup != null && _infoPopup.gameObject.activeSelf == true) _infoPopup.gameObject.SetActive(false);
            GameObject[] root = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObject in root)
            {
                if (rootObject.name == "AudioManager")
                    rootObject.transform.Find("MainMenuMusic").GetComponent<AudioSource>().Stop();
            }
            _audioManager.transform.Find("Music").GetComponent<MusicList>().PlayMusic();
            string name = GetComponent<MusicList>().GetTrackName();
            //if(name != null)
            _editTray.transform.Find("MusicField").Find("CurrentMusic").GetComponent<TextMeshProUGUI>().text = name;
            EditModeTrayResize();
            GameAnalyticsManager.OpenSoulHome();
        }

        public void OnDisable()
        {
            GameObject[] root = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObject in root)
            {
                if (rootObject.name == "AudioManager")
                    rootObject.transform.Find("MainMenuMusic").GetComponent<AudioSource>().Play();
            }
            GetComponent<MusicList>().StopMusic();
        }

        public void SetRoomName(GameObject room)
        {
            if (room != null)
            {
                _roomName.gameObject.SetActive(true);
                string roomName = room.GetComponent<RoomData>().RoomInfo.Id.ToString();
                _roomName.GetComponent<TextMeshProUGUI>().text = "Huone " + roomName;
            }
            else _roomName.gameObject.SetActive(false);
        }

        public void ExitSoulHome()
        {
            if (_exitPending || _confirmPopupOpen) return;
            if (_soulHomeTower.ChangedFurnitureList.Count > 0)
            {
                _exitPending = true;
                _confirmPopupOpen = true;
                _confirmPopup.SetActive(true);
                _confirmPopup.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Sielunkodissa on tallentamattomia muutoksia. \n\n"
                + "Poistumalla hylkäät tallentamattomat muutokset. \n\n"
                + "Haluatko silti poistua? ";
                _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(ConfirmExitFalse);
                _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.AddListener(ConfirmExitTrue);
                _confirmPopup.transform.Find("AcceptButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sulje sielunkoti";
            }
            else
            {
                if(_soulHomeTower.EditingMode)_soulHomeTower.ToggleEdit();
                WindowManager.Get().GoBack();
            }
        }
        public void ConfirmExitFalse() { ConfirmExit(false); }
        public void ConfirmExitTrue() { ConfirmExit(true); }

        public void ConfirmExit(bool confirm)
        {
            if (confirm)
            {
                _soulHomeTower.DeselectFurniture();
                _mainScreen.ResetChanges();
                CloseConfirmPopup(PopupType.Exit);
                if (_soulHomeTower.EditingMode) _soulHomeTower.ToggleEdit();
                _exitPending = false;
                WindowManager.Get().GoBack();
            }
            else
            {
                CloseConfirmPopup(PopupType.Exit);
                _exitPending = false;
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
                    _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(ConfirmEditCloseFalse);
                    _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.AddListener(ConfirmEditCloseTrue);
                    _confirmPopup.transform.Find("AcceptButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sulje muokkaustila";
                }
                else
                    _soulHomeTower.ToggleEdit();
            }
        }
        public void EditModeTrayHandle(bool open)
        {
            if (!open)
            {
                _editButton.interactable = true;
                _editTray.GetComponent<RectTransform>().pivot = new(0, 0.5f);
                _editTray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                _editButton.interactable = false;
                _editTray.GetComponent<RectTransform>().pivot = new(1, 0.5f);
                _editTray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }

        public void EditModeTrayResize()
        {
            float width;
            if (transform.Find("UICanvas").GetComponent<RectTransform>().rect.width / 2 < 1000) width = transform.Find("UICanvas").GetComponent<RectTransform>().rect.width / 2;
            else width =1000;
            _editTray.GetComponent<RectTransform>().sizeDelta = new Vector2(width,0);
        }

        public void NextMusicTrack()
        {
            string name = _audioManager.transform.Find("Music").GetComponent<MusicList>().NextTrack();
            if (name != null)
                _editTray.transform.Find("MusicField").Find("CurrentMusic").GetComponent<TextMeshProUGUI>().text = name;
        }

        public void PrevMusicTrack()
        {
            string name = _audioManager.transform.Find("Music").GetComponent<MusicList>().PrevTrack();
            if (name != null)
                _editTray.transform.Find("MusicField").Find("CurrentMusic").GetComponent<TextMeshProUGUI>().text = name;
        }

        public void ConfirmEditCloseFalse() { ConfirmEditClose(false); }
        public void ConfirmEditCloseTrue() { ConfirmEditClose(true); }

        public void ConfirmEditClose(bool confirm)
        {
            if (confirm)
            {
                _soulHomeTower.DeselectFurniture();
                _mainScreen.ResetChanges();
                CloseConfirmPopup(PopupType.EditClose);
                _soulHomeTower.ToggleEdit();
                _audioManager.transform.Find("RevertChanges").GetComponent<AudioSource>().Play();
            }
            else
            {
                CloseConfirmPopup(PopupType.EditClose);
            }
        }

        private void CloseConfirmPopup(PopupType type)
        {
            if(type is PopupType.Exit)
            {
                _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveListener(ConfirmExitFalse);
                _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.RemoveListener(ConfirmExitTrue);
            }
            else if (type is PopupType.EditClose)
            {
                _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveListener(ConfirmEditCloseFalse);
                _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.RemoveListener(ConfirmEditCloseTrue);
            }
            _confirmPopupOpen = false;
            _confirmPopup.SetActive(false);
        }

        public void ShowInfoPopup(string popupText)
        {
            _infoPopup.ActivatePopUp(popupText);
        }

    }
}
