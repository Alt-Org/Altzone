using System;
using TMPro;
using UnityEngine;

namespace QuantumUser.Scripts.UI.Views
{
    public class GameUiAnnouncementHandler : MonoBehaviour
    {
        //UICountDownView
        [SerializeField] private GameObject _view;
        [SerializeField] private TextMeshProUGUI _announcerText;

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

        // Update is called once per frame
        private void Update()
        {

        }
    }
}


