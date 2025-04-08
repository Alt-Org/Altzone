using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.BattleUiShared;

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

        public BattleUiMultiOrientationElement LocalPlayerMultiOrientationElement;
        public BattleUiMultiOrientationElement TeammateMultiOrientationElement;

        private void OnDisable()
        {
            LocalPlayerMultiOrientationElement.gameObject.SetActive(false);
            TeammateMultiOrientationElement.gameObject.SetActive(false);
        }

        public void SetInfo(PlayerType playerType, string playerName, int[] characterIds)
        {
            GameUiPlayerInfoComponent playerInfoComponent;

            if (playerType == PlayerType.LocalPlayer)
            {
                LocalPlayerMultiOrientationElement.gameObject.SetActive(true);
                playerInfoComponent = LocalPlayerMultiOrientationElement.GetActiveGameObject().GetComponent<GameUiPlayerInfoComponent>();
            }
            else
            {
                TeammateMultiOrientationElement.gameObject.SetActive(true);
                playerInfoComponent = TeammateMultiOrientationElement.GetActiveGameObject().GetComponent<GameUiPlayerInfoComponent>();
            }

            if (playerInfoComponent == null) return;

            // Setting player name
            playerInfoComponent.PlayerName.text = playerName;

            // Setting character icons
            for (int i = 0; i < characterIds.Length; i++)
            {
                playerInfoComponent.CharacterButtons[i].SetCharacterIcon(characterIds[i]);
            }
        }

        public Button[] GetLocalPlayerCharacterButtons()
        {
            GameUiPlayerInfoComponent playerInfoComponent = LocalPlayerMultiOrientationElement.GetActiveGameObject().GetComponent<GameUiPlayerInfoComponent>();

            if (playerInfoComponent == null) return null;

            Button[] buttons = new Button[3];
            for (int i = 0; i < playerInfoComponent.CharacterButtons.Length; i++)
            {
                buttons[i] = playerInfoComponent.CharacterButtons[i].ButtonComponent;
            }

            return buttons;
        }
    }
}

