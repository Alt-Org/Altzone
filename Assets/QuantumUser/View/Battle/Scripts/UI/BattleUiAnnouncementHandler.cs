using UnityEngine;
using TMPro;

namespace Battle.View.UI
{
    public class BattleUiAnnouncementHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _view;
        [SerializeField] private TextMeshProUGUI _announcerText;

        public bool IsVisible => _view.activeSelf;

        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        //countdown from x to 0 based on GameSessionState
        public void SetCountDownNumber(int countDown)
        {
            _announcerText.text = $"{countDown}";
        }

        public void ShowEndOfCountDownText()
        {
            _announcerText.text = "GO!";
        }

        public void ShowGameOverText()
        {
            _announcerText.text = "Game Over!";
        }

        public void ClearAnnouncerTextField()
        {
            _announcerText.text = "";
        }

        private void Awake()
        {
            _announcerText.text = "";
        }
    }
}


