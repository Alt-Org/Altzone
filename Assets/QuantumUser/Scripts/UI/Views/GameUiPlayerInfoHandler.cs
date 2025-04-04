using Altzone.Scripts.BattleUi;
using TMPro;
using UnityEngine;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles Battle Ui player info functionality.
    /// </summary>
    public class GameUiPlayerInfoHandler : MonoBehaviour
    {
        [Header("Horizontal configuration")]
        [SerializeField] private TMP_Text _playerNameHorizontal;
        [SerializeField] private GameUiCharacterButtonHandler[] _characterButtonsHorizontal;

        [Header("Vertical configuration")]
        [SerializeField] private TMP_Text _playerNameVertical;
        [SerializeField] private GameUiCharacterButtonHandler[] _characterButtonsVertical;

        [Header("Movable UI")]
        public BattleUiElement MovableUiElement;

        private void OnDisable()
        {
            MovableUiElement.gameObject.SetActive(false);
        }

        public void SetInfo(string playerName, int[] characterIds)
        {
            // Setting player name
            if (MovableUiElement.IsHorizontal)
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
                if (MovableUiElement.IsHorizontal)
                {
                    _characterButtonsHorizontal[i].SetCharacterIcon(characterIds[i]);
                }
                else
                {
                    _characterButtonsVertical[i].SetCharacterIcon(characterIds[i]);
                }
            }

            // Making ui element visible
            MovableUiElement.gameObject.SetActive(true);
        }
    }
}

