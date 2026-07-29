using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    public class CloseGamePopupHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _popup;
        [SerializeField] private Button _returnButton;
        [SerializeField] private Button _closeButton;

        private bool _waitingResponse;

        public delegate void RequestCloseGameEvent();
        public static event RequestCloseGameEvent OnRequestCloseGameEvent;

        public delegate void CloseGameResponce(bool accepted);
        public static event CloseGameResponce OnCloseGameResponce;

        public static bool RequestCloseGame()
        {
            if(OnRequestCloseGameEvent != null)
            {
                OnRequestCloseGameEvent.Invoke();
                return true;

            }
            return false;
        }

        private void OnEnable()
        {
            OnRequestCloseGameEvent += OpenPopUp;
            _returnButton.onClick.AddListener(ReturnToGame);
            _closeButton.onClick.AddListener(CloseGame);
        }

        private void OnDisable()
        {
            OnRequestCloseGameEvent -= OpenPopUp;
            _returnButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
            if(_waitingResponse) OnCloseGameResponce?.Invoke(false);
        }

        private void OpenPopUp()
        {
            _popup.SetActive(true);
            _waitingResponse = true;
        }

        private void CloseGame()
        {
            _waitingResponse = false;
            _popup.SetActive(false);
            OnCloseGameResponce?.Invoke(true);
        }

        private void ReturnToGame()
        {
            _waitingResponse = false;
            _popup.SetActive(false);
            OnCloseGameResponce?.Invoke(false);
        }
    }
}
