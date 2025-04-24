using UnityEngine;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles Battle Ui player info functionality.
    /// </summary>
    public class BattleUiPlayerInfoHandler : MonoBehaviour
    {
        [SerializeField] private BattleUiController _uiController;
        [SerializeField] private BattleUiMultiOrientationElement _localPlayerMultiOrientationElement;
        [SerializeField] private BattleUiMultiOrientationElement _localTeammateMultiOrientationElement;

        public enum PlayerType
        {
            LocalPlayer,
            LocalTeammate,
        }

        public bool IsVisible => _localPlayerMultiOrientationElement.gameObject.activeSelf;
        public BattleUiMultiOrientationElement LocalPlayerMultiOrientationElement   => _localPlayerMultiOrientationElement;
        public BattleUiMultiOrientationElement LocalTeammateMultiOrientationElement => _localTeammateMultiOrientationElement;

        public void SetShow(bool show)
        {
            _localPlayerMultiOrientationElement.gameObject.SetActive(show);
            _localTeammateMultiOrientationElement.gameObject.SetActive(show);
        }

        public void SetInfo(PlayerType playerType, string playerName, int[] characterIds)
        {
            // Getting player info component from multi orientation element
            BattleUiPlayerInfoComponent playerInfoComponent =
                (playerType == PlayerType.LocalPlayer
                    ? _localPlayerMultiOrientationElement
                    : _localTeammateMultiOrientationElement
                ).GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();

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

                if (playerType == PlayerType.LocalTeammate) continue;

                // Adding listener to button press
                int characterNumber = i;
                characterButton.ButtonComponent.onClick.RemoveAllListeners();
                characterButton.ButtonComponent.onClick.AddListener(() => _uiController.GameViewController.UiInputOnCharacterSelected(characterNumber));
            }
        }
    }
}
