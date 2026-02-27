using Altzone.Scripts.ModelV2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.CreateRoom
{
    /// <summary>
    /// Used for selecting the game map and room name when creating a custom game from the Battle Popup. Room name is the selected emotional situation's name.
    /// </summary>
    public class MapAndRoomNameSelector : MonoBehaviour
    {
        [Header("Map selector")]
        [SerializeField] private TextMeshProUGUI _currentMapText;
        [SerializeField] private Button _nextMapButton;
        [SerializeField] private Button _previousMapButton;

        [Header("Room name selector")]
        [SerializeField] private TextMeshProUGUI _currentNameText;
        [SerializeField] private Button _nextNameButton;
        [SerializeField] private Button _previousNameButton;

        [Header("Reference sheet")]
        [SerializeField] private BattleMapReference _battleMapReference;

        private int _selectedMapIndex = 0;
        private int _selectedNameIndex = 0;

        private int _oldIndex;

        public BattleMap SelectedBattleMap { get; private set; }
        public MapEmotionalSituation SelectedEmotionalSituation { get; private set; }

        public void Initialize()
        {
            SelectedBattleMap = _battleMapReference.Maps[_selectedMapIndex];
            SelectedEmotionalSituation = SelectedBattleMap.EmotionalSituations[_selectedNameIndex];

            _nextMapButton.onClick.AddListener(OnNextMapClicked);
            _previousMapButton.onClick.AddListener(OnPreviousMapClicked);
            _currentMapText.text = SelectedBattleMap.MapName;

            _nextNameButton.onClick.AddListener(OnNextNameClicked);
            _previousNameButton.onClick.AddListener(OnPreviousNameClicked);
            _currentNameText.text = SelectedEmotionalSituation.SituationName;
        }

        private void OnNextMapClicked()
        {
            _oldIndex = _selectedMapIndex; 

            // Checking if there is a next map index or if we should get the first map
            if (_selectedMapIndex + 1  < _battleMapReference.Maps.Count)
            {
                _selectedMapIndex++;
            }
            else
            {
                _selectedMapIndex = 0;
            }

            SelectMap();
        }

        private void OnPreviousMapClicked()
        {
            _oldIndex = _selectedMapIndex;

            if (_selectedMapIndex - 1 >= 0)
            {
                _selectedMapIndex--;
            }
            else
            {
                _selectedMapIndex = _battleMapReference.Maps.Count - 1;
            }

            SelectMap();
        }

        private void SelectMap()
        {
            // Selecting the new map
            SelectedBattleMap = _battleMapReference.Maps[_selectedMapIndex];
            _currentMapText.text = SelectedBattleMap.MapName;

            if (_oldIndex != _selectedMapIndex) // TODO: once there is more maps than 1 we can remove this if statement and the oldIndex int
            {
                // Resetting the emotional situation
                _selectedNameIndex = 0;
                SelectName();
            }
        }

        private void OnNextNameClicked()
        {
            if (_selectedNameIndex + 1 < SelectedBattleMap.EmotionalSituations.Length)
            {
                _selectedNameIndex++;
            }
            else
            {
                _selectedNameIndex = 0;
            }

            SelectName();
        }

        private void OnPreviousNameClicked()
        {
            if (_selectedNameIndex - 1 >= 0)
            {
                _selectedNameIndex--;
            }
            else
            {
                _selectedNameIndex = SelectedBattleMap.EmotionalSituations.Length - 1;
            }

            SelectName();
        }

        private void SelectName()
        {
            SelectedEmotionalSituation = SelectedBattleMap.EmotionalSituations[_selectedNameIndex];
            _currentNameText.text = SelectedEmotionalSituation.SituationName;
        }
    }
}
