using UnityEngine;
using UnityEngine.UI;

using Battle.View.Game;
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

        [SerializeField] private BattleGameViewController _viewController;

        public BattleUiMultiOrientationElement LocalPlayerMultiOrientationElement;
        public BattleUiMultiOrientationElement TeammateMultiOrientationElement;

        public void SetInfo(PlayerType playerType, string playerName, int[] characterIds)
        {
            BattleUiPlayerInfoComponent playerInfoComponent;

            // Activating multi orientation gameObject and getting player info component
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

            // Initializing character buttons
            for (int i = 0; i < characterIds.Length; i++)
            {
                BattleUiCharacterButtonComponent characterButton = playerInfoComponent.CharacterButtons[i];

                // Setting character icon
                characterButton.SetCharacterIcon(characterIds[i]);

                // Setting if button is enabled
                characterButton.ButtonComponent.enabled = playerType == PlayerType.LocalPlayer;

                if (playerType == PlayerType.LocalPlayerTeammate) return;

                // Adding listener to button press
                int characterNumber = i;
                characterButton.ButtonComponent.onClick.AddListener(() => _viewController.OnCharacterSelected(characterNumber));
            }
        }

        private void OnDisable()
        {
            LocalPlayerMultiOrientationElement.gameObject.SetActive(false);
            TeammateMultiOrientationElement.gameObject.SetActive(false);
        }
    }
}

