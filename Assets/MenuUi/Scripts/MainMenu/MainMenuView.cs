using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _clanName;

        public string PlayerName
        {
            set => _playerName.text = value;
        }

        public string ClanName
        {
            set => _clanName.text = value;
        }

        public void ResetView()
        {
            PlayerName = string.Empty;
            ClanName = string.Empty;
        }
    }
}
