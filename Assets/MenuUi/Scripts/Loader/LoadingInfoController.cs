using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LogInStatus
{
    None,
    LogIn,
    CheckSettingsData,
    FetchPlayerData,
    FetchClanData,
    Finished,
    MovingToMain
}

namespace MenuUi.Scripts.Loader
{
    public class LoadingInfoController : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _loadingText;
        [SerializeField]
        private TextMeshProUGUI _infoText;
        [SerializeField]
        private Button _moveToMainButton;

        private LogInStatus _status = LogInStatus.None;

        public LogInStatus Status { get => _status;
            set
            {
                _status = value;
                if (_status == LogInStatus.Finished)
                {
                    _moveToMainButton.interactable = true;
                    _loadingText.gameObject.SetActive(false);
                }
                else if (_status == LogInStatus.MovingToMain)
                {
                    _moveToMainButton.interactable = false;
                    _loadingText.gameObject.SetActive(false);
                }
                else _moveToMainButton.interactable = false;

                SetInfoText(_status);
            }
        }

        public delegate void MoveToMainEvent();
        public event MoveToMainEvent OnMoveToMain;

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable()
        {
            _moveToMainButton.onClick.AddListener(MoveToMain);
            StartCoroutine(LoadingProgress());
        }

        public void LogIn()
        {
            gameObject.SetActive(true);
            if (!_loadingText.gameObject.activeSelf) StartCoroutine(LoadingProgress());
            Status = LogInStatus.LogIn;
        }

        public void SetInfoText(LogInStatus type)
        {
            switch (type)
            {
                case LogInStatus.LogIn:
                    _infoText.text = "Kirjaudutaan sisään";
                    break;
                case LogInStatus.FetchPlayerData:
                    _infoText.text = "Haetaan pelaajan tietoja";
                    break;
                case LogInStatus.FetchClanData:
                    _infoText.text = "Haetaan klaanin tietoja";
                    break;
                case LogInStatus.Finished:
                    _infoText.text = "Paina tästä siirtyäksesi pääikkunaan.";
                    break;
                case LogInStatus.MovingToMain:
                    _infoText.text = "Siirrytään pääikkunaan";
                    break;
                case LogInStatus.CheckSettingsData:
                    _infoText.text = "Tarkistetaan asetuksia";
                    break;
                default:
                    break;
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);

        }

        public void LoadReady()
        {
            Status = LogInStatus.Finished;
            StopCoroutine(LoadingProgress());
            _loadingText.gameObject.SetActive(false);
        }

        public void MoveToMain()
        {
            if (Status != LogInStatus.Finished) return;
            Status = LogInStatus.MovingToMain;
            OnMoveToMain?.Invoke();
        }

        public void DisableInfo()
        {
            gameObject.SetActive(false);
        }

        private IEnumerator LoadingProgress()
        {
            _loadingText.gameObject.SetActive(true);
            int i=0;
            while (true)
            {
                yield return new WaitForSeconds(1);
                i++;
                if (i == 4) i = 1;
                _loadingText.text = "Ladataan"+ new string('.',i);
            }
        }
    }
}
