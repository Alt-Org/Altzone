using System;
using System.Collections;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;


namespace MenuUi.Scripts.Lobby
{
    public static partial class SignalBus
    {
        public delegate void RandomSelectedCharactersRequested();
        public static event RandomSelectedCharactersRequested OnRandomSelectedCharactersRequested;
        public static void OnRandomSelectedCharactersRequestedSignal()
        {
            OnRandomSelectedCharactersRequested?.Invoke();
        }

        public delegate void DefenceGalleryEditModeRequested();
        public static event DefenceGalleryEditModeRequested OnDefenceGalleryEditModeRequested;
        public static void OnDefenceGalleryEditModeRequestedSignal()
        {
            OnDefenceGalleryEditModeRequested?.Invoke();
        }
    }


    public class SelectedCharactersPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _container;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _defenceGalleryButton;
        [SerializeField] private Button _selectRandomButton;


        public IEnumerator ShowPopup(Action<bool?> callback)
        {
            bool? openBattlePopupAfterwards = null;

            _closeButton.onClick.RemoveAllListeners();
            _defenceGalleryButton.onClick.RemoveAllListeners();
            _selectRandomButton.onClick.RemoveAllListeners();

            OpenPopup();

            _closeButton.onClick.AddListener(() => {
                ClosePopup();
                openBattlePopupAfterwards = false;
                });

            _defenceGalleryButton.onClick.AddListener(() =>
            {
                ClosePopup();
                FindObjectOfType<SwipeUI>(true).CurrentPage = 1; // Changing swipe page to defence gallery
                SignalBus.OnDefenceGalleryEditModeRequestedSignal();
                openBattlePopupAfterwards = false;
            });

            _selectRandomButton.onClick.AddListener(() =>
            {
                SelectRandomCharacters();
                openBattlePopupAfterwards = true;
            });

            yield return new WaitUntil(() => openBattlePopupAfterwards.HasValue);
            callback(openBattlePopupAfterwards);
        }

        public void OpenPopup()
        {
            _container.SetActive(true);
        }


        public void ClosePopup()
        {
            _container.SetActive(false);
        }


        public void SelectRandomCharacters()
        {
            SignalBus.OnRandomSelectedCharactersRequestedSignal();
            ClosePopup();
        }
    }
}

