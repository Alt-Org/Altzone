using UnityEngine;

using Altzone.Scripts.Lobby;

namespace Battle.View.UI
{
    public class BattleUiGameOverHandler : MonoBehaviour
    {
        //UICountDownView
        [SerializeField] private GameObject _view;

        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        public void OnButtonClickExitGame()
        {
            LobbyManager.ExitQuantum();
        }

        //only for testing
        public void OnButtonClickRestartGame()
        {

        }
    }
}

