/// @file BattleUiPlayerInfoHandler.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiPlayerInfoHandler} class which handles %Battle %UI player and teammate info visual functionality..
/// </summary>
///
/// This script:<br/>
/// Handles %Battle %UI player and teammate info visual functionality through BattleUiPlayerInfoComponent.

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Altzone usings
using Altzone.Scripts.BattleUiShared;

// Battle View usings
using Battle.View.Game;

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
        public bool IsVisible => _isVisible;

        /// <value>Is the Player %UI element visible or not.</value>
        public bool IsVisiblePlayer => _isVisiblePlayer;

        /// <value>Is the Teammate %UI element visible or not.</value>
        public bool IsVisibleTeammate => _isVisibleTeammate;

        /// <value>Public getter for #_localPlayerMultiOrientationElement.</value>
        public BattleUiMultiOrientationElement LocalPlayerMultiOrientationElement   => _localPlayerMultiOrientationElement;

        /// <value>Public getter for #_localTeammateMultiOrientationElement.</value>
        public BattleUiMultiOrientationElement LocalTeammateMultiOrientationElement => _localTeammateMultiOrientationElement;

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        ///
        /// Updates both elements using the master flag and their individual states.
        /// Acts as a master switch without overriding individual visibility settings.
        ///
        /// <param name="show">True/False : visible / not visible.</param>
        public void SetShow(bool show)
        {
            _isVisible = show;
            _localPlayerMultiOrientationElement   .gameObject.SetActive(_isVisible && _isVisiblePlayer);
            _localTeammateMultiOrientationElement .gameObject.SetActive(_isVisible && _isVisibleTeammate);
        }

        /// <summary>
        /// Sets the Player %UI element visibility.
        /// </summary>
        ///
        /// Updates only the Player element.
        /// Visible only if both the master and Player flags are true.
        ///
        /// <param name="show">True/False : visible / not visible.</param>
        public void SetShowPlayer(bool show)
        {
            _isVisiblePlayer = show;
            _localPlayerMultiOrientationElement.gameObject.SetActive(_isVisible && _isVisiblePlayer);
        }

        /// <summary>
        /// Sets the Teammate %UI element visibility.
        /// </summary>
        ///
        /// Updates only the Teammate element.
        /// Visible only if both the master and Teammate flags are true.
        ///
        /// <param name="show">True/False : visible / not visible.</param>
        public void SetShowTeammate(bool show)
        {
            _isVisibleTeammate = show;
            _localTeammateMultiOrientationElement.gameObject.SetActive(_isVisible && _isVisibleTeammate);
        }

        /// <summary>
        /// Sets the player's info to BattleUiPlayerInfo prefab through BattleUiPlayerInfoComponent.
        /// </summary>
        ///
        /// <param name="playerType">The PlayerType which info to set.</param>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="characterIds">An array of the character IDs of the players selected characters.</param>
        /// <param name="characterClasses">An array of the character classes of the players selected characters.</param>
        /// <param name="characterDefenceNumbers">Array of defence values for each character.</param>
        /// <param name="data">The BattleUiMovableElementData for this UI element.</param>
        public void SetInfo(PlayerType playerType, string playerName, int[] characterIds, int[] characterClasses, float[] characterDefenceNumbers, BattleUiMovableElementData data)
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
                BattleUiCharacterFrameComponent characterFrameHandler = playerInfoComponent.FrameComponents[i];

                // Setting character icon
                characterButton.SetCharacterIcon(characterIds[i]);

                // Setting frame
                characterFrameHandler.SetCharacterFrame((BattlePlayerCharacterClass)characterClasses[i]);

                // Setting defence number
                characterButton.SetDefenceNumber(characterDefenceNumbers[i]);

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
        /// Checks if the character selection was for the local player.<br/>
        /// Calls SetSelected on all character buttons to set them to false, then sets the selected character's to true.
        /// </summary>
        ///
        /// <param name="slot">The slot of the player in question.</param>
        /// <param name="characterNumber">The character number of the selected character.</param>
        public void SetSelected(BattlePlayerSlot slot, int characterNumber)
        {
            if (slot != BattleGameViewController.LocalPlayerSlot) return;

            BattleUiPlayerInfoComponent playerInfoComponent = GetPlayerInfoComponent(slot);

            if (playerInfoComponent == null) return;

            foreach (BattleUiCharacterButtonComponent character in playerInfoComponent.CharacterButtons)
            {
                character.SetSelected(false);
            }

            if (characterNumber == -1) return;

            playerInfoComponent.CharacterButtons[characterNumber].SetSelected(true);
        }

        /// <summary>
        /// Calls SetDamageFill on the character button of the correct players correct character.
        /// </summary>
        ///
        /// <param name="slot">The slot of the player in question.</param>
        /// <param name="characterNumber">The character number of the character in question.</param>
        /// <param name="defencePercentage">The defence percentage of the character in question.</param>
        public void UpdateDefenceVisual(BattlePlayerSlot slot, int characterNumber, float defencePercentage)
        {
            BattleUiPlayerInfoComponent playerInfoComponent = GetPlayerInfoComponent(slot);

            if (playerInfoComponent == null) return;

            playerInfoComponent.CharacterButtons[characterNumber].SetDamageFill(defencePercentage);
        }

        /// <summary>
        /// Retrieves the BattleUiPlayerInfoComponent for either the local player or their teammate.
        /// </summary>
        ///
        /// <param name="slot">The slot of the player in question.</param>
        ///
        /// <returns>The BattleUiPlayerInfoComponent for the specified player.</returns>
        public BattleUiPlayerInfoComponent GetPlayerInfoComponent(BattlePlayerSlot slot)
        {
            if (slot == BattleGameViewController.LocalPlayerSlot)
            {
                return _localPlayerMultiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();
            }
            else
            {
                return _localTeammateMultiOrientationElement.GetActiveGameObject().GetComponent<BattleUiPlayerInfoComponent>();
            }
        }

        /// <value>Is the %UI element visible or not.</value>
        private bool _isVisible = false;

        /// <value>Is the Player %UI element visible or not.</value>
        private bool _isVisiblePlayer = false;

        /// <value>Is the Teammate %UI element visible or not.</value>
        private bool _isVisibleTeammate = false;
    }
}
