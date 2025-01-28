using MenuUI.Scripts.Lobby.InLobby;
using UnityEngine;


namespace MenuUi.Scripts.Lobby
{
    public static partial class SignalBus
    {
        public delegate void RandomSelectedCharactersRequested();
        public static event RandomSelectedCharactersRequested OnRandomSelectedCharactersRequested;
        public static void OnRandomSelectedCharactersRequestedSignal()
        {
            OnRandomSelectedCharactersRequested?.Invoke();
        }
    }


    public class SelectedCharactersPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _container;

        public void OpenPopup()
        {
            _container.SetActive(true);
        }


        public void ClosePopup()
        {
            _container.SetActive(false);
        }


        public void SelectRandomCharacters()
        {
            SignalBus.OnRandomSelectedCharactersRequestedSignal();
            ClosePopup();
        }
    }
}

