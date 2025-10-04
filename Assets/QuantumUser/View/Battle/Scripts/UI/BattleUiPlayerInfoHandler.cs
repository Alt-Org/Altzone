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
        public bool IsVisible => _isVisible;

        public bool IsVisiblePlayer => _isVisiblePlayer;
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
        /// <param name="playerName">The player's name.</param>
        /// <param name="characterIds">The player's selected characters CharacterIds as a int array.</param>
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
        /// Updates character health visual through BattleUiPlayerInfoComponent.
        /// </summary>
        ///
        /// <param name="slot">The player's BattlePlayerSlot who's character health visual to update.</param>
        /// <param name="characterNumber">The character number which health visual to update.</param>
        /// <param name="healthPercentage">The updated health percentage for the character.</param>
        public void UpdateHealthVisual(BattlePlayerSlot slot, int characterNumber, float healthPercentage)
        {
            BattleUiPlayerInfoComponent playerInfoComponent = GetPlayerInfoComponent(slot);

            if (playerInfoComponent == null) return;

            playerInfoComponent.CharacterButtons[characterNumber].SetDamageFill(healthPercentage);
        }

        public void UpdateDefenceVisual(BattlePlayerSlot slot, int characterNumber, float defenceValue)
        {
            BattleUiPlayerInfoComponent playerInfoComponent = GetPlayerInfoComponent(slot);

            if (playerInfoComponent == null) return;

            playerInfoComponent.CharacterButtons[characterNumber].SetDefenceNumber(defenceValue);
        }

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

        private bool _isVisible         = false;
        private bool _isVisiblePlayer   = false;
        private bool _isVisibleTeammate = false;
    }
}
