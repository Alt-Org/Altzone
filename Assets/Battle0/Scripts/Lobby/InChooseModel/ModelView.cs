using System;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Battle0.Scripts.Lobby.InChooseModel
{
    /// <summary>
    /// <c>CharacterModel</c> view.
    /// </summary>
    public class ModelView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private InputField playerName;
        [SerializeField] private Button continueButton;

        [SerializeField] private Transform leftPane;
        [SerializeField] private Transform rightPane;
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private GameObject curPrefab;

        private Button[] _buttons;
        private Text[] _labels;

        private void Awake()
        {
            _buttons = leftPane.GetComponentsInChildren<Button>();
            _labels = rightPane.GetComponentsInChildren<Text>();
            // Index 0 is not used but must be valid in order to prevent NRE at runtime.
            // - we rely that prefab index is same as CharacterModel Id and table is setup accordingly.
            curPrefab = prefabs[0];
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
            foreach (var prefab in prefabs)
            {
                prefab.SetActive(false);
            }
        }

        public void SetCharacters(List<IBattleCharacter> characters, int currentCharacterId)
        {
            CurrentCharacterId = currentCharacterId;
            for (var i = 0; i < characters.Count; ++i)
            {
                var character = characters[i];
                var button = _buttons[i];
                button.interactable = true;
                button.SetCaption(character.Name);
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
            _labels[++i].text = $"{character.Name}";
            _labels[++i].text = $"MainDefence:\r\n{character.MainDefence}";
            _labels[++i].text = $"Speed:\r\n{character.Speed}";
            _labels[++i].text = $"Resistance:\r\n{character.Resistance}";
            _labels[++i].text = $"Attack:\r\n{character.Attack}";
            _labels[++i].text = $"Defence:\r\n{character.Defence}";
            SetCharacterPrefab(character);
        }

        private void SetCharacterPrefab(IBattleCharacter character)
        {
            curPrefab.SetActive(false);
            // HACK: we assume that prefabs are arranged by this same id! This worked well when all models were hard coded.
            curPrefab = prefabs[character.CustomCharacterModelId];
            curPrefab.SetActive(true);
        }
    }
}