using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class NamePanel : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private TMP_Text _characterName;

        private void OnEnable()
        {
            _characterName.text = _controller.GetCurrentCharacterName();
        }
    }
}
