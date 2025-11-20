/// @file BattleUiLoadScreenHandler.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiLoadScreentHandler} class, which handles setting the visibility of the battle loading screen and updating it whenever a player connects to the game.
/// </summary>
///
/// This script:<br/>
/// Handles setting the visibility of the battle loading screen and updating it whenever a player connects to the game.

// Unity usings
using UnityEngine;
using TMPro;

// Quantum usings
using Quantum;

// Altzone usings
using MenuUi.Scripts.Lobby.SelectedCharacters;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.View.UI
{
    public class BattleUiLoadScreenHandler : MonoBehaviour
    {
        /// @anchor BattleUiLoadScreenHandler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to the GameObject which can be used to hide or show the loading screen.</summary>
        /// @ref BattleUiLoadScreenHandler-SerializeFields
        [SerializeField] private GameObject _loadingScreen;

        /// <summary>[SerializeField] References to the character slot controllers which are used to show the connection status and selected characters of players in the game.</summary>
        /// @ref BattleUiLoadScreenHandler-SerializeFields
        [SerializeField] private BattlePopupCharacterSlotController[] _characterSlotControllers;

        /// <summary>[SerializeField] References to the character frame handlers which are used to set the correct UI frame for each character icon.</summary>
        /// @ref BattleUiLoadScreenHandler-SerializeFields
        [SerializeField] private BattleUiCharacterFrameComponent[] _characterFrameComponents;

        /// <summary>[SerializeField] References to the player name UI elements.</summary>
        /// @ref BattleUiLoadScreenHandler-SerializeFields
        [SerializeField] private TextMeshProUGUI[] _playerNames;

        /// @}

        /// <summary>
        /// If the player which connected is part of the game, this method:<br/>
        /// Enables the UI element for that player on the loading screen.<br/>
        /// Calls the correct BattlePopupCharacterSlotController::SetCharacters method to set the character icons.<br/>
        /// Changes the alpha of the player name on the loading screen to indicate the player has connected.
        /// </summary>
        ///
        /// <param name="playerSlot">The slot of the player.</param>
        /// <param name="characterIds">An array of the character IDs of the players selected characters.</param>
        public void PlayerConnected(BattlePlayerSlot playerSlot, int[] characterIds, int[] characterClasses)
        {
            int slotIndex = playerSlot switch
            {
                BattlePlayerSlot.Slot1 => 0,
                BattlePlayerSlot.Slot2 => 1,
                BattlePlayerSlot.Slot3 => 2,
                BattlePlayerSlot.Slot4 => 3,
                _ => -1,
            };

            int frameIndex = playerSlot switch
            {
                BattlePlayerSlot.Slot1 => 0,
                BattlePlayerSlot.Slot2 => 3,
                BattlePlayerSlot.Slot3 => 6,
                BattlePlayerSlot.Slot4 => 9,
                _ => -1,
            };

            if (slotIndex == -1 || frameIndex == -1) return;

            _characterSlotControllers[slotIndex].gameObject.SetActive(true);
            _characterSlotControllers[slotIndex].SetCharacters(characterIds);

            for (int i = 0; i < characterClasses.Length; i++)
            {
                _characterFrameComponents[frameIndex + i].SetCharacterFrame((BattlePlayerCharacterClass)characterClasses[i]);
            }

            _playerNames[(int)playerSlot - 1].alpha = 1;
        }

        /// <summary>
        /// For each player slot for which PlayerType is set to Player, the corresponding player name UI element is enabled and set to that player's name.
        /// </summary>
        ///
        /// <param name="playerSlotTypes"></param>
        /// <param name="playerNames"></param>
        public void Show(BattleParameters.PlayerType[] playerSlotTypes, FixedArray<QString64> playerNames)
        {
            for (int i = 0; i < playerSlotTypes.Length; i++)
            {
                if (playerSlotTypes[i] == BattleParameters.PlayerType.Player)
                {
                    _playerNames[i].text = playerNames[i];
                    _playerNames[i].gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Disables the loading screen UI element entirely.
        /// </summary>
        public void Hide()
        {
            _loadingScreen.SetActive(false);
        }
    }
}
