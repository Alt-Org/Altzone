using UnityEngine;
using TMPro;

namespace Battle.View.UI
{
    public class BattleUiAnnouncementHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _view;
        [SerializeField] private TextMeshProUGUI _announcerText;

        public enum TextType
        {
            Loading,
            WaitingForPlayers,
            EndOfCountdown,
        }

        public bool IsVisible => _view.activeSelf;

        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        public void SetText(TextType textType)
        {
            _announcerText.text = textType switch
            {
                TextType.Loading            => "Loading...",
                TextType.WaitingForPlayers  => "Waiting for\nplayers...",
                TextType.EndOfCountdown     => "GO!",

                _ => string.Format("Unimplemented text type {0}.", textType),
            };
        }

        //countdown from x to 0 based on GameSessionState
        public void SetCountDownNumber(int countDown)
        {
            _announcerText.text = $"{countDown}";
        }

        public void ClearAnnouncerTextField()
        {
            _announcerText.text = "";
        }

        private void Awake()
        {
            ClearAnnouncerTextField();
        }
    }
}


