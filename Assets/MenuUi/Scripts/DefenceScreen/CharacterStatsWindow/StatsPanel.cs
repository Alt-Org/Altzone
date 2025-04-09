using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Controls visual functionality of CharacterStatsPanel.
    /// </summary>
    public class StatsPanel : MonoBehaviour
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _lockImage;

        [SerializeField] private GameObject _statPage;
        [SerializeField] private GameObject _infoPage;
        private StatsWindowController _controller;

        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();

            SetCharacterImage();
                        

            if(_controller.IsCurrentCharacterLocked())
            {
                _lockImage.gameObject.SetActive(true);
            }
            else
            {
                _lockImage.gameObject.SetActive(false);
            }
        }

        private void SetCharacterImage()
        {
            Sprite sprite = _controller.GetCurrentCharacterSprite();

            if (sprite != null)
            {
                _characterImage.sprite = sprite;
            }
        }


       
        public void SwitchPage()
        {
            if (_statPage != null) _statPage.SetActive(false);
            if (_infoPage != null) _infoPage.SetActive(true);
        }
        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}
