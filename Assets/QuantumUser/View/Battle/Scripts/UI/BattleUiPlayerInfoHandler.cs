using UnityEngine;

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

        [SerializeField] private BattleUiController _uiController;

        public BattleUiMultiOrientationElement LocalPlayerMultiOrientationElement;
        public BattleUiMultiOrientationElement TeammateMultiOrientationElement;
        public bool IsVisible => LocalPlayerMultiOrientationElement.gameObject.activeSelf;

        public void SetShow(bool show)
        {
            LocalPlayerMultiOrientationElement.gameObject.SetActive(show);
            TeammateMultiOrientationElement.gameObject.SetActive(show);
        }

        public void SetInfo(PlayerType playerType, string playerName, int[] characterIds)
        {
            BattleUiPlayerInfoComponent playerInfoComponent;

            // Getting player info component from multi orientation element
            if (playerType == PlayerType.LocalPlayer)
            {
                playerInfoComponent = LocalPlayerMultiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();
            }
            else
            {
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

                if (playerType == PlayerType.LocalPlayerTeammate) continue;

                // Adding listener to button press
                int characterNumber = i;
                characterButton.ButtonComponent.onClick.RemoveAllListeners();
                characterButton.ButtonComponent.onClick.AddListener(() => _uiController.GameViewController.OnCharacterSelected(characterNumber));
            }
        }
    }
}
