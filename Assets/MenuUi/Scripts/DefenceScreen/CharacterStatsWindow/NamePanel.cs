using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Gets Characters name for character name panel in info pop up
    /// </summary>
    public class NamePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _characterName;
        private StatsWindowController _controller;

        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();
            _characterName.text = _controller.GetCurrentCharacterName();
        }
    }
}
