using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Battle0.Scripts.Lobby.InChooseModel
{
    /// <summary>
    /// <c>CharacterModel</c> view - example using furniture prefabs as we do not have proper player prefabs for character selection yet.
    /// </summary>
    public class ModelView : MonoBehaviour
    {
        [SerializeField] private Transform VerticalContentPanel;
        [SerializeField] private Transform HorizontalContentPanel;

        [SerializeField] private bool _isReady;

        private Button[] _buttons;
        public CharacterSlot[] _CurSelectedCharacterSlot { get; private set; }
        private CharacterSlot[] CharacterSlot;


        public delegate void CurrentCharacterIdChangedHandler(string newCharacterId);
        public event CurrentCharacterIdChangedHandler OnCurrentCharacterIdChanged;
        public bool IsReady => _isReady;

        private string _currentCharacterId;



        private void Awake()
        {
            _buttons = VerticalContentPanel.GetComponentsInChildren<Button>();
            _CurSelectedCharacterSlot = HorizontalContentPanel.GetComponentsInChildren<CharacterSlot>();
            CharacterSlot = VerticalContentPanel.GetComponentsInChildren<CharacterSlot>();


            LoadAndCachePrefabs();
        }

        private void LoadAndCachePrefabs()
        {
            var gameConfig = GameConfig.Get();
            var playerPrefabs = gameConfig.PlayerPrefabs;
            var prefabs = playerPrefabs._playerPrefabs;
            for (var prefabIndex = 0; prefabIndex < prefabs.Length; ++prefabIndex)
            {
                var playerPrefab = GameConfig.Get().PlayerPrefabs.GetPlayerPrefab(prefabIndex.ToString());

                Debug.Log($"prefabIndex {prefabIndex} playerPrefab {playerPrefab.name}");

            }
            _isReady = true;
        }



        public string CurrentCharacterId
        {
            get => _currentCharacterId;
            private set
            {
                if (_currentCharacterId != value)
                {
                    _currentCharacterId = value;
                    OnCurrentCharacterIdChanged?.Invoke(_currentCharacterId); 
                }
            }
        }

        public void Reset()
        {
            foreach (var button in _buttons)
            {
                button.gameObject.SetActive(false);
            }
            foreach (var characterSlot in CharacterSlot)
            {
                characterSlot.gameObject.SetActive(false);
            }

        }
        public void SetCharacters(List<BattleCharacter> characters, string currentCharacterId)
        {
            CurrentCharacterId = currentCharacterId;

            for (var i = 0; i < characters.Count && i < _buttons.Length && i < CharacterSlot.Length; ++i)
            {
                var character = characters[i];
                var button = _buttons[i];
                var characterSlot = CharacterSlot[i];

                button.gameObject.SetActive(true);
                button.interactable = true;
                button.SetCaption(character.Name);

                characterSlot.gameObject.SetActive(true);

                if (currentCharacterId == character.CustomCharacterId)
                {
                    if (_CurSelectedCharacterSlot.Length > 0)
                    {
                        button.transform.SetParent(_CurSelectedCharacterSlot[0].transform, false);
                    }
                }

                var parentChangeMonitor = button.GetComponent<DraggableCharacter>();
                parentChangeMonitor.OnParentChanged += newParent =>
                {
                    if (newParent == _CurSelectedCharacterSlot[0].transform)
                    {
                        CurrentCharacterId = character.CustomCharacterId;
                    }
                };



            }

        }

    }
}
