using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.MainMenu
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