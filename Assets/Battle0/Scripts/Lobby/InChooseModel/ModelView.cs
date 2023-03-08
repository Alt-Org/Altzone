using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Temp;
using UnityEngine;
using UnityEngine.UI;

namespace Battle0.Scripts.Lobby.InChooseModel
{
    /// <summary>
    /// <c>CharacterModel</c> view - example using furniture prefabs as we do not have proper player prefabs for character selection yet.
    /// </summary>
    public class ModelView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private InputField playerName;
        [SerializeField] private Button continueButton;

        [SerializeField] private Transform leftPane;
        [SerializeField] private Transform rightPane;
        [SerializeField] private Transform _prefabsRoot;
        [SerializeField] private MonoBehaviour[] _prefabs;
        [SerializeField] private MonoBehaviour _curPrefab;
        [SerializeField] private bool _isReady;

        private Button[] _buttons;
        private Text[] _labels;

        public bool IsReady => _isReady;

        private void Awake()
        {
            _buttons = leftPane.GetComponentsInChildren<Button>();
            _labels = rightPane.GetComponentsInChildren<Text>();
            LoadPrefabsAsync();
        }

        private void LoadPrefabsAsync()
        {
            // This will be async if custom character models are loaded from network.
            var store = Storefront.Get();
            var characterModels = store.GetAllCharacterClassModels();
            var maxIndex = characterModels.Max(x => x.Id);
            _prefabs = new MonoBehaviour[1 + maxIndex];
            var position = _prefabsRoot.position;
            foreach (var characterModel in characterModels)
            {
                Debug.Log($"Character: {characterModel}");
                var playerPrefab = GameConfig.Get().PlayerPrefabs.GetPlayerPrefab(characterModel.Id);
                var instance = Instantiate(playerPrefab, _prefabsRoot);
                if (instance == null)
                {
                    continue;
                }
                instance.gameObject.SetActive(false);
                _prefabs[characterModel.Id] = instance;
            }
            _isReady = true;
        }

        public string Title
        {
            get => titleText.text;
            set => titleText.text = value;
        }

        public string PlayerName
        {
            get => playerName.text;
            set => playerName.text = value;
        }

        public int CurrentCharacterId { get; private set; }

        public Action ContinueButtonOnClick
        {
            set { continueButton.onClick.AddListener(() => value()); }
        }

        public void Reset()
        {
            Title = string.Empty;
            PlayerName = string.Empty;
            foreach (var label in _labels)
            {
                label.text = string.Empty;
            }
            foreach (var prefab in _prefabs)
            {
                if (prefab == null)
                {
                    continue;
                }
                prefab.gameObject.SetActive(false);
            }
            foreach (var button in _buttons)
            {
                button.gameObject.SetActive(false);
            }
            _curPrefab = null;
        }

        public void SetCharacters(List<IBattleCharacter> characters, int currentCharacterId)
        {
            Debug.Log($"characters {characters.Count} current {currentCharacterId}");
            CurrentCharacterId = currentCharacterId;
            for (var i = 0; i < characters.Count && i < _buttons.Length; ++i)
            {
                var character = characters[i];
                var button = _buttons[i];
                button.gameObject.SetActive(true);
                button.interactable = true;
                //button.SetCaption(character.Name);
                button.onClick.AddListener(() =>
                {
                    CurrentCharacterId = character.CustomCharacterModelId;
                    ShowCharacter(character);
                });
                if (currentCharacterId == character.CustomCharacterModelId)
                {
                    ShowCharacter(character);
                }
            }
        }

        private void ShowCharacter(IBattleCharacter character)
        {
            var i = -1;
            var characterName = character.Name == character.CharacterClassName
                ? character.Name
                : $"{character.Name} [{character.CharacterClassName}]";
            _labels[++i].text = $"{characterName}";
            _labels[++i].text = $"MainDefence:\r\n{character.MainDefence}";
            _labels[++i].text = $"Speed:\r\n{character.Speed}";
            _labels[++i].text = $"Resistance:\r\n{character.Resistance}";
            _labels[++i].text = $"Attack:\r\n{character.Attack}";
            _labels[++i].text = $"Defence:\r\n{character.Defence}";
            SetCharacterPrefab(character);
        }

        private void SetCharacterPrefab(IBattleCharacter character)
        {
            if (_curPrefab != null)
            {
                _curPrefab.gameObject.SetActive(false);
            }
            _curPrefab = _prefabs[character.PlayerPrefabId];
            if (_curPrefab != null)
            {
                _curPrefab.gameObject.SetActive(true);
            }
        }
    }
}
