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

        [SerializeField] private GameObject currentPage;
        [SerializeField] private GameObject targetPage;
        private StatsWindowController _controller;

        private GameObject previousPage;

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
            if (currentPage != null) currentPage.SetActive(false);
            if (targetPage != null) targetPage.SetActive(true);
        }

        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}
