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
        private TextMeshProUGUI _furnitureName;
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
        public TextMeshProUGUI FurnitureName { get => _furnitureName;}

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
            if(GameAnalyticsManager.Instance !=null) GameAnalyticsManager.Instance.OpenSoulHome();
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

        public void SetFurniture(Furniture furniture)
        {
            if (furniture != null)
            {
                if(!_soulHomeTower.EditingMode || _soulHomeTower.Rotated)FurnitureName.gameObject.SetActive(true);
                string furnitureName = furniture.Name;
                FurnitureName.GetComponent<TextMeshProUGUI>().text = furnitureName;
            }
            else FurnitureName.gameObject.SetActive(false);
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
                + "Poistuaksesi muutokset pitää tallentaa tai hylätä. \n\n"
                + "Haluatko silti poistua? ";
                _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(ConfirmExitFalse);
                _confirmPopup.transform.Find("SecondaryAcceptButton").GetComponent<Button>().onClick.AddListener(ConfirmExitTrueWithRevert);
                _confirmPopup.transform.Find("SecondaryAcceptButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Palauta Muutokset";
                _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.AddListener(ConfirmExitTrueWithSave);
                _confirmPopup.transform.Find("AcceptButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Tallenna Muutokset";
            }
            else
            {
                if(_soulHomeTower.EditingMode)_soulHomeTower.ToggleEdit();
                WindowManager.Get().GoBack();
            }
        }
        public void ConfirmExitFalse() { ConfirmExit(false); }
        public void ConfirmExitTrueWithSave() { ConfirmExit(true, true); }
        public void ConfirmExitTrueWithRevert() { ConfirmExit(true, false); }

        public void ConfirmExit(bool confirm, bool save = false)
        {
            if (confirm)
            {
                _soulHomeTower.DeselectFurniture();
                if(save)_mainScreen.SaveChanges();
                else _mainScreen.ResetChanges();
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
                    + "Sulkeaksesi muokkaustilan tallentamattomat muutokset pitää tallentaa tai hylätä. \n\n"
                    + "Mitä haluat tehdä? ";
                    _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(ConfirmEditCloseFalse);
                    _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.AddListener(ConfirmEditCloseTrueWithSave);
                    _confirmPopup.transform.Find("AcceptButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Tallenna Muutokset";
                    _confirmPopup.transform.Find("SecondaryAcceptButton").GetComponent<Button>().onClick.AddListener(ConfirmEditCloseTrueWithRevert);
                    _confirmPopup.transform.Find("SecondaryAcceptButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Palauta Muutokset";
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
        public void ConfirmEditCloseTrueWithRevert() { ConfirmEditClose(true, false); }
        public void ConfirmEditCloseTrueWithSave() { ConfirmEditClose(true, true); }

        public void ConfirmEditClose(bool confirm, bool save = false)
        {
            if (confirm)
            {
                _soulHomeTower.DeselectFurniture();
                if (save) _mainScreen.SaveChanges();
                else _mainScreen.ResetChanges();
                CloseConfirmPopup(PopupType.EditClose);
                _soulHomeTower.ToggleEdit();
                if(save) _audioManager.transform.Find("SaveChanges").GetComponent<AudioSource>().Play();
                else _audioManager.transform.Find("RevertChanges").GetComponent<AudioSource>().Play();
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
                _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.RemoveListener(ConfirmExitTrueWithSave);
                _confirmPopup.transform.Find("SecondaryAcceptButton").GetComponent<Button>().onClick.RemoveListener(ConfirmExitTrueWithRevert);
            }
            else if (type is PopupType.EditClose)
            {
                _confirmPopup.transform.Find("CancelButton").GetComponent<Button>().onClick.RemoveListener(ConfirmEditCloseFalse);
                _confirmPopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.RemoveListener(ConfirmEditCloseTrueWithSave);
                _confirmPopup.transform.Find("SecondaryAcceptButton").GetComponent<Button>().onClick.RemoveListener(ConfirmEditCloseTrueWithRevert);
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
