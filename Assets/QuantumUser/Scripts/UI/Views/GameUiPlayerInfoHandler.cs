using Altzone.Scripts.BattleUiShared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles Battle Ui player info functionality.
    /// </summary>
    public class GameUiPlayerInfoHandler : MonoBehaviour
    {
        public enum PlayerType
        {
            LocalPlayer,
            LocalPlayerTeammate,
        }

        [Header("Horizontal configuration")]
        [SerializeField] private TMP_Text _playerNameHorizontal;
        [SerializeField] private GameUiCharacterButtonComponent[] _characterButtonsHorizontal;

        [Header("Vertical configuration")]
        [SerializeField] private TMP_Text _playerNameVertical;
        [SerializeField] private GameUiCharacterButtonComponent[] _characterButtonsVertical;

        [Header("Movable UI")]
        public BattleUiMovableElement MovableUiElement;

        private void OnDisable()
        {
            MovableUiElement.gameObject.SetActive(false);
        }

        public void SetInfo(PlayerType playerType, string playerName, int[] characterIds)
        {
            // Setting player name
            if (true)
            {
                _playerNameHorizontal.text = playerName;
            }
            else
            {
                _playerNameVertical.text = playerName;
            }

            // Setting character icons
            for (int i = 0; i < characterIds.Length; i++)
            {
                if (true)
                {
                    _characterButtonsHorizontal[i].SetCharacterIcon(characterIds[i]);
                    _characterButtonsHorizontal[i].ButtonComponent.enabled = playerType == PlayerType.LocalPlayer;
                }
                else
                {
                    _characterButtonsVertical[i].SetCharacterIcon(characterIds[i]);
                    _characterButtonsVertical[i].ButtonComponent.enabled = playerType == PlayerType.LocalPlayer;
                }
            }

            // Making ui element visible
            MovableUiElement.gameObject.SetActive(true);
        }

        public Button[] GetCharacterButtons()
        {
            Button[] buttons = new Button[3];
            if (true)
            {
                for (int i = 0; i < _characterButtonsHorizontal.Length; i++)
                {
                    buttons[i] = _characterButtonsHorizontal[i].ButtonComponent;
                }
            }
            else
            {
                for (int i = 0; i < _characterButtonsHorizontal.Length; i++)
                {
                    buttons[i] = _characterButtonsVertical[i].ButtonComponent;
                }
            }

            return buttons;
        }
    }
}

