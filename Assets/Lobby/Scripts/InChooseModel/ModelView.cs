using Altzone.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InChooseModel
{
    /// <summary>
    /// Model view.
    /// </summary>
    public class ModelView : MonoBehaviour
    {
        public Text titleText;
        public InputField playerName;
        public Button continueButton;

        [SerializeField] private Transform leftPane;
        [SerializeField] private Transform rightPane;
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private GameObject curPrefab;

        private Button[] buttons;
        private Text[] labels;

        private void Awake()
        {
            buttons = leftPane.GetComponentsInChildren<Button>();
            labels = rightPane.GetComponentsInChildren<Text>();
        }

        public Button getButton(int buttonIndex)
        {
            return buttons[buttonIndex];
        }

        public void hideCharacter()
        {
            foreach (var label in labels)
            {
                label.text = "";
            }
        }

        public void showCharacter(CharacterModel character)
        {
            var i = -1;
            labels[++i].text = $"{character.Name}";
            labels[++i].text = $"MainDefence:\r\n{character.MainDefence}";
            labels[++i].text = $"Speed:\r\n{character.Speed}";
            labels[++i].text = $"Resistance:\r\n{character.Resistance}";
            labels[++i].text = $"Attack:\r\n{character.Attack}";
            labels[++i].text = $"Defence:\r\n{character.Defence}";
            setCharacterPrefab(character);
        }

        private void setCharacterPrefab(CharacterModel character)
        {
            curPrefab.SetActive(false);
            curPrefab = prefabs[character.Id];
            curPrefab.SetActive(true);
        }
    }
}