using System;
using System.Collections;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;
using SignalBus = MenuUi.Scripts.CharacterGallery.SignalBus;

namespace MenuUi.Scripts.Lobby
{
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
                FindObjectOfType<SwipeUI>().CurrentPage = 1; // Changing swipe page to defence gallery
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

