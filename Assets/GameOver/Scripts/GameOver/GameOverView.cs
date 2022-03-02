using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.GameOver
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private Text _winnerInfo1;
        [SerializeField] private Text _winnerInfo2;
        [SerializeField] private Button _continueButton;

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

        public void Reset()
        {
            _winnerInfo1.text = string.Empty;
            _winnerInfo2.text = string.Empty;
            _continueButton.interactable = false;
        }

        public Button ContinueButton => _continueButton;
    }
}