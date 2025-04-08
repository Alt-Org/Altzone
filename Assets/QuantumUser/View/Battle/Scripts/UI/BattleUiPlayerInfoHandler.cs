using UnityEngine;
using UnityEngine.UI;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles Battle Ui player info functionality.
    /// </summary>
    public class BattleUiPlayerInfoHandler : MonoBehaviour
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
            BattleUiPlayerInfoComponent playerInfoComponent;

            if (playerType == PlayerType.LocalPlayer)
            {
                LocalPlayerMultiOrientationElement.gameObject.SetActive(true);
                playerInfoComponent = LocalPlayerMultiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();
            }
            else
            {
                TeammateMultiOrientationElement.gameObject.SetActive(true);
                playerInfoComponent = TeammateMultiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();
            }

            if (playerInfoComponent == null) return;

            // Setting player name
            playerInfoComponent.PlayerName.text = playerName;

            // Setting character icons
            for (int i = 0; i < characterIds.Length; i++)
            {
                playerInfoComponent.CharacterButtons[i].SetCharacterIcon(characterIds[i]);
                playerInfoComponent.CharacterButtons[i].ButtonComponent.enabled = playerType == PlayerType.LocalPlayer;
            }
        }

        public Button[] GetLocalPlayerCharacterButtons()
        {
            BattleUiPlayerInfoComponent playerInfoComponent = LocalPlayerMultiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();

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

