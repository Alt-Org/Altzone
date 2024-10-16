﻿using System;
using System.Collections.Generic;
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
            LoadAndCachePrefabs();
        }

        private void LoadAndCachePrefabs()
        {
            var gameConfig = GameConfig.Get();
            var playerPrefabs = gameConfig.PlayerPrefabs;
            var prefabs = playerPrefabs._playerPrefabs;
            _prefabs = new MonoBehaviour[prefabs.Length];
            for (var prefabIndex = 0; prefabIndex < prefabs.Length; ++prefabIndex)
            {
                var playerPrefab = GameConfig.Get().PlayerPrefabs.GetPlayerPrefab(prefabIndex);
                var instance = Instantiate(playerPrefab, _prefabsRoot);
                if (instance == null)
                {
                    continue;
                }
                Debug.Log($"prefabIndex {prefabIndex} playerPrefab {playerPrefab.name}");
                instance.gameObject.SetActive(false);
                _prefabs[prefabIndex] = instance;
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

        public CharacterID CurrentCharacterId { get; private set; }

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

        public void SetCharacters(List<BattleCharacter> characters, CharacterID currentCharacterId)
        {
            Debug.Log($"characters {characters.Count} current {currentCharacterId}");
            CurrentCharacterId = currentCharacterId;
            for (var i = 0; i < characters.Count && i < _buttons.Length; ++i)
            {
                var character = characters[i];
                var button = _buttons[i];
                button.gameObject.SetActive(true);
                button.interactable = true;
                button.SetCaption(character.Name);
                button.onClick.AddListener(() =>
                {
                    CurrentCharacterId = character.CharacterID;
                    ShowCharacter(character);
                });
                if (currentCharacterId == character.CharacterID)
                {
                    ShowCharacter(character);
                }
            }
        }

        private void ShowCharacter(BattleCharacter character)
        {
            var i = -1;
            _labels[++i].text = $"{character.Name}";
            //_labels[++i].text = $"GestaltCycle:\r\n{character.CharacterClass.Name}";
            _labels[++i].text = $"Speed:\r\n{character.Speed}";
            _labels[++i].text = $"Resistance:\r\n{character.Resistance}";
            _labels[++i].text = $"Attack:\r\n{character.Attack}";
            _labels[++i].text = $"Defence:\r\n{character.Defence}";
            var prefabIndex = PhotonBattle.GetPrefabIndex(character, -1);
            SetCharacterPrefab(prefabIndex);
        }

        private void SetCharacterPrefab(int prefabIndex)
        {
            if (_curPrefab != null)
            {
                _curPrefab.gameObject.SetActive(false);
            }
            _curPrefab = prefabIndex >= 0 ? _prefabs[prefabIndex] : null;
            if (_curPrefab != null)
            {
                _curPrefab.gameObject.SetActive(true);
            }
        }
    }
}
