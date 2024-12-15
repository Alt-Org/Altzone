using System;
using TMPro;
using UnityEngine;

namespace QuantumUser.Scripts.UI.Views
{
    public class UIGameAnnouncementHandler : MonoBehaviour
    {

        //UICountDownView
        [SerializeField] public TextMeshProUGUI announcerText;


        private void Awake()
        {
            announcerText.text = "";
        }

        // Update is called once per frame
        void Update()
        {

        }
        //countdown from x to 0 based on GameSessionState
        public void SetCountDownNumber(int countDown)
        {
            announcerText.text = $"{countDown}";
        }

        public void ShowEndOfCountDownText()
        {
            announcerText.text = "GO!";
        }

        public void ShowGameOverText()
        {
            announcerText.text = "Game Over!";
        }

        public void ClearAnnouncerTextField()
        {
            announcerText.text = "";
        }
    }
}


