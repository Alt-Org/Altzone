using MenuUI.Scripts.Lobby.InLobby;
using UnityEngine;


namespace MenuUi.Scripts.CharacterGallery
{
    public class SelectedCharactersPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _container;
        [SerializeField] private InLobbyController _inLobbyController;
        [SerializeField] private ModelController _modelController;

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
            _modelController.SetRandomSelectedCharactersToEmptySlots();
            ClosePopup();
            _inLobbyController.ToggleWindow();
        }
    }
}

