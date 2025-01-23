using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.DefenceScreen.CharacterStatsWindow;
using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class StoryPanel : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private TMP_Text _characterStory;

        private void OnEnable()
        {
            _characterStory.text = _controller.GetCurrentCharacterStory();
        }
    }
}
