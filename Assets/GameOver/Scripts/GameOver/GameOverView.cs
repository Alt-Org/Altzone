using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameOver.Scripts.GameOver
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private Text _winnerInfo1;
        [SerializeField] private Text _winnerInfo2;
        [SerializeField] private Text _loserInfo;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _restartButton;

        public bool RoomOpen;

        public string WinnerInfo1
        {
            get => _winnerInfo1.text;
            set => _winnerInfo1.text = value;
        }

        public string WinnerInfo2
        {
            get => _winnerInfo2.text;
            set => _winnerInfo2.text = value;
        }
          public string LoserInfo
        {
            get => _loserInfo.text;
            set => _loserInfo.text = value;
        }

        public Action ContinueButtonOnClick
        {
            set { _continueButton.onClick.AddListener(() => value()); }
        }

        public Action RestartButtonOnClick
        {
            set { _restartButton.onClick.AddListener(() => value()); }
        }

        public void EnableContinueButton() => _continueButton.interactable = true;

        public void EnableRestartButton() => _restartButton.interactable = true;
        public void DisableRestartButton() => _restartButton.interactable = false;

        public void Reset()
        {
            Debug.Log($"{name}");
            _winnerInfo1.text = string.Empty;
            _winnerInfo2.text = string.Empty;
            DisableButtons();
        }

        private void DisableButtons()
        {
            _continueButton.interactable = false;
            _restartButton.interactable = false;
        }
    }
}
