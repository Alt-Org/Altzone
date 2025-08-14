/// @file BattleUiPlayerInfoHandler.cs
/// <summary>
/// Has a class BattleUiPlayerInfoHandler which handles %Battle %UI player and teammate info visual functionality..
/// </summary>
///
/// This script:<br/>
/// Handles %Battle %UI player and teammate info visual functionality through BattleUiPlayerInfoComponent.

using UnityEngine;

using Altzone.Scripts.BattleUiShared;
using Battle.View.Game;
using Quantum;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">PlayerInfo @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles %Battle %UI player and teammate info visual functionality through BattleUiPlayerInfoComponent.
    /// </summary>
    public class BattleUiPlayerInfoHandler : MonoBehaviour
    {
        /// @anchor BattleUiPlayerInfoHandler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to BattleUiController.</summary>
        /// @ref BattleUiPlayerInfoHandler-SerializeFields
        [SerializeField] private BattleUiController _uiController;

        /// <summary>[SerializeField] Reference to the BattleUiMultiOrientationElement script which is attached to the local player's BattleUiPlayerInfo prefab.</summary>
        /// @ref BattleUiPlayerInfoHandler-SerializeFields
        [SerializeField] private BattleUiMultiOrientationElement _localPlayerMultiOrientationElement;

        /// <summary>[SerializeField] Reference to the BattleUiMultiOrientationElement script which is attached to the local player's teammate's BattleUiPlayerInfo prefab.</summary>
        /// @ref BattleUiPlayerInfoHandler-SerializeFields
        [SerializeField] private BattleUiMultiOrientationElement _localTeammateMultiOrientationElement;

        /// @}

        /// <summary>
        /// Helper enum to differentiate between local player and local player's teammate when calling #SetInfo method.
        /// </summary>
        public enum PlayerType
        {
            LocalPlayer,
            LocalTeammate,
        }

        /// <value>Is the %UI element visible or not.</value>
        public bool IsVisible => _localPlayerMultiOrientationElement.gameObject.activeSelf;

        /// <value>Public getter for #_localPlayerMultiOrientationElement.</value>
        public BattleUiMultiOrientationElement LocalPlayerMultiOrientationElement   => _localPlayerMultiOrientationElement;

        /// <value>Public getter for #_localTeammateMultiOrientationElement.</value>
        public BattleUiMultiOrientationElement LocalTeammateMultiOrientationElement => _localTeammateMultiOrientationElement;

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        ///
        /// <param name="show">If the %UI element should be visible or not.</param>
        public void SetShow(bool show)
        {
            _localPlayerMultiOrientationElement.gameObject.SetActive(show);
            _localTeammateMultiOrientationElement.gameObject.SetActive(show);
        }

        /// <summary>
        /// Sets the player's info to BattleUiPlayerInfo prefab through BattleUiPlayerInfoComponent.
        /// </summary>
        ///
        /// <param name="playerType">The PlayerType which info to set.</param>
        /// <param name="playerName">The player's name.</param>
        /// <param name="characterIds">The player's selected characters CharacterIds as a int array.</param>
        /// <param name="data">The BattleUiMovableElementData for this UI element.</param>
        public void SetInfo(PlayerType playerType, string playerName, int[] characterIds, BattleUiMovableElementData data)
        {
            // Selecting correct multiorientation element
            BattleUiMultiOrientationElement multiOrientationElement = playerType == PlayerType.LocalPlayer
                ? _localPlayerMultiOrientationElement
                : _localTeammateMultiOrientationElement;

            // Setting BattleUiMovableElementData to multi orientation element
            if (data != null) multiOrientationElement.SetData(data);

            // Getting player info component from multiorientation element
            BattleUiPlayerInfoComponent playerInfoComponent = multiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();
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
                characterButton.EventSender.onClick.RemoveAllListeners();
                if (playerType == PlayerType.LocalPlayer)
                {
                    characterButton.EventSender.onClick.AddListener(() => _uiController.GameViewController.UiInputOnCharacterSelected(characterNumber));
                }
            }
        }

        /// <summary>
        /// Updates character health visual through BattleUiPlayerInfoComponent.
        /// </summary>
        ///
        /// <param name="slot">The player's BattlePlayerSlot who's character health visual to update.</param>
        /// <param name="characterNumber">The character number which health visual to update.</param>
        /// <param name="healthPercentage">The updated health percentage for the character.</param>
        public void UpdateHealthVisual(BattlePlayerSlot slot, int characterNumber, float healthPercentage)
        {
            BattleUiPlayerInfoComponent playerInfoComponent = null;

            if (slot == BattleGameViewController.LocalPlayerSlot)
            {
                playerInfoComponent = _localPlayerMultiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();
            }
            else
            {
                playerInfoComponent = _localTeammateMultiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();
            }

            if (playerInfoComponent == null) return;

            playerInfoComponent.CharacterButtons[characterNumber].SetDamageFill(healthPercentage);
        }
    }
}
