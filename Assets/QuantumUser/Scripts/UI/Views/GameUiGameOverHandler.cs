using UnityEngine;

namespace QuantumUser.Scripts.UI.Views
{
    public class GameUiGameOverHandler : MonoBehaviour
    {
        //UICountDownView
        [SerializeField] private GameObject _view;

        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        public void OnButtonClickExitGame()
        {

        }

        //only for testing
        public void OnButtonClickRestartGame()
        {

        }
    }
}

