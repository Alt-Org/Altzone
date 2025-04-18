using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts;
using TMPro;
using MenuUi.Scripts.Window;
using UnityEngine.UI;
using Altzone.Scripts.GA;
using MenuUi.Scripts.Audio;

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
        private ConfirmPopupController _confirmPopup;
        [SerializeField]
        private PopupController _infoPopup;
        [SerializeField]
        private Button _editButton;
        [SerializeField]
        private TextMeshProUGUI _editButtonText;
        [SerializeField]
        private GameObject _editTray;
        [SerializeField]
        private JukeBoxSoulhomeHandler _jukeBoxPopup;
        [SerializeField]
        private Button _openJukeBox;
        [SerializeField]
        private TextMeshProUGUI _musicName;
        [SerializeField]
        private AudioManager _audioManager;

        private FurnitureList _furnitureList = new();

        private bool _confirmPopupOpen = false;
        private bool _exitPending = false;

        public bool ExitPending { get => _exitPending; }
        public bool ConfirmPopupOpen { get => _confirmPopupOpen; }
        public TextMeshProUGUI FurnitureName { get => _furnitureName; }
        public FurnitureList FurnitureList { get => _furnitureList; }

        // Start is called before the first frame update
        void Start()
        {
            if (ServerManager.Instance.Clan != null)
            {
                _clanName.text = $"Klaanin {ServerManager.Instance.Clan.name} Sielunkoti";
            }
            EditModeTrayResize();
            _audioManager = AudioManager.Instance;
            _editButton.onClick.AddListener(()=>EditModeToggle());
            _openJukeBox.onClick.AddListener(()=>_jukeBoxPopup.ToggleJokeBoxScreen(true));
        }

        public void OnEnable()
        {
            //if (_infoPopup != null && _infoPopup.gameObject.activeSelf == true) _infoPopup.gameObject.SetActive(false);
            GameObject[] root = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObject in root)
            {
                if (rootObject.name == "AudioManager")
                    rootObject.GetComponent<MainMenuAudioManager>()?.StopMusic();
            }
            _musicName.text = AudioManager.Instance?.PlayMusic(MusicSection.SoulHome);
            EditModeTrayResize();
            if (GameAnalyticsManager.Instance != null) GameAnalyticsManager.Instance.OpenSoulHome();
            JukeboxController.OnChangeJukeBoxSong += SetSongName;
        }

        public void OnDisable()
        {
            GameObject[] root = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObject in root)
            {
                if (rootObject.name == "AudioManager")
                    rootObject.GetComponent<MainMenuAudioManager>()?.PlayMusic();
            }
            AudioManager.Instance?.StopMusic();
            _jukeBoxPopup.StopJukebox();
            _jukeBoxPopup.ToggleJokeBoxScreen(false);
            JukeboxController.OnChangeJukeBoxSong -= SetSongName;
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

        public void AddFurniture(Furniture furniture)
        {
            if (furniture == null) return;
            else
            {
                if (furniture.Id == "-1") return;
                if (string.IsNullOrWhiteSpace(furniture.Name)) return;
            }

            _furnitureList.Add(furniture);
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
                _confirmPopup.OpenPopUp(PopupType.Exit, ConfirmExitFalse, ConfirmExitTrueWithSave, ConfirmExitTrueWithRevert);
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
                    _confirmPopup.OpenPopUp(PopupType.EditClose, ConfirmEditCloseFalse, ConfirmEditCloseTrueWithSave, ConfirmEditCloseTrueWithRevert);
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
                _editButtonText.text = "Avaa\nMuokkaustila";
            }
            else
            {
                //_editButton.interactable = false;
                //_editTray.GetComponent<RectTransform>().pivot = new(1, 0.5f);
                //_editTray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                _editButtonText.text = "Sulje\nMuokkaustila";
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
            string name = _audioManager.NextMusicTrack();
            if (name != null)
                _musicName.text = name;
        }

        public void PrevMusicTrack()
        {
            string name = _audioManager.PrevMusicTrack();
            if (name != null)
                _musicName.text = name;
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
                if(save) _audioManager.PlaySfxAudio("SaveChanges");
                else _audioManager.PlaySfxAudio("RevertChanges");
            }
            else
            {
                CloseConfirmPopup(PopupType.EditClose);
            }
        }

        private void CloseConfirmPopup(PopupType type)
        {
            _confirmPopup.ClosePopUp();
            _confirmPopupOpen = false;
        }

        public void ShowInfoPopup(string popupText)
        {
            SignalBus.OnChangePopupInfoSignal(popupText);
        }

        private void SetSongName(JukeboxSong song)
        {
            _musicName.text = song.songName;
        }

        public bool CheckInteractableStatus()
        {
            if (_exitPending) return false;
            if (_confirmPopupOpen) return false;
            if (_jukeBoxPopup.JukeBoxOpen) return false;

            return true;
        }
    }
}
