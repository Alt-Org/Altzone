using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace GameOver.Scripts.GameOver
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private Text _winnerInfo1;
        [SerializeField] private Text _winnerInfo2;
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

        public Action ContinueButtonOnClick
        {
            set { _continueButton.onClick.AddListener(() => value()); }
        }

        public Action RestartButtonOnClick
        {
            set { _restartButton.onClick.AddListener(() => value()); }
            
        }

        public void Reset()
        {
            _winnerInfo1.text = string.Empty;
            _winnerInfo2.text = string.Empty;
            DisableButtons();
        }

        public void DisableButtons()
        {
            _continueButton.interactable = false;
            _restartButton.interactable = false;
        }

        public void EnableButtons()
        {

            _continueButton.interactable = true;
            if (PhotonNetwork.IsMasterClient)
                _restartButton.interactable = true;
            if  (!PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.IsOpen)
                _restartButton.interactable = true;

        }
      /*  private void Start()
        {
            RoomOpen = false; 
        }
        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient || RoomOpen == true)
            {
                 _restartButton.interactable = true;
            }
        }*/
    }
}
