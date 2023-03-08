using UnityEngine;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void OnEnable()
        {
            _view.Reset();
            _view.PlayerName = "Player";
            _view.ClanName = "Clan";
        }
    }
}