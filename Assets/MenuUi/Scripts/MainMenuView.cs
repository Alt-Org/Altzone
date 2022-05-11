using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.Scripts
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Text _playerName;

        public string PlayerName
        {
            set => _playerName.text = value;
        }
    }
}