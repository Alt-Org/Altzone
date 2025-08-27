using UnityEngine;
using TMPro;

using Altzone.Scripts.BattleUiShared;

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
            if (_debugmode) return;
            _announcerText.text = textType switch
            {
                TextType.Loading            => "Loading...",
                TextType.WaitingForPlayers  => "Waiting for\nplayers to connect...",
                TextType.EndOfCountdown     => "GO!",

                _ => string.Format("Unimplemented text type {0}.", textType),
            };
        }

        //countdown from x to 0 based on GameSessionState
        public void SetCountDownNumber(int countDown)
        {
            if (_debugmode) return;
            _announcerText.text = $"{countDown}";
        }

        public void ClearAnnouncerTextField()
        {
            if (_debugmode) return;
            _announcerText.text = "";
        }

        public void SetDebugtext(string text)
        {
            _announcerText.text = text;
            _announcerText.color = Color.red;
            _announcerText.textWrappingMode = TextWrappingModes.Normal;
            _announcerText.fontSize *= 0.5f;
            _debugmode = true;
        }

        private bool _debugmode = false;

        private void Awake()
        {
            ClearAnnouncerTextField();
        }
    }
}


