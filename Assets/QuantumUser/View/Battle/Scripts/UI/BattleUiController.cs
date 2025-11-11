/// @file BattleUiController.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiController} class which controls the %Battle %UI.
/// </summary>
///
/// This script:<br/>
/// Holds references to all of the @ref UIHandlerReferences and @ref DebugUIHandlerReferences, and the BattleGameViewController script.

// Unity usings
using UnityEngine;

// Battle View usings
using Battle.View.Game;

namespace Battle.View.UI
{
    /// <summary>
    /// Main controller for %Battle %UI.<br/>
    /// BattleGameViewController accesses the @uihandlerslink scripts through this class.<br/>
    /// <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.
    /// </summary>
    ///
    /// Holds references to all of the @ref UIHandlerReferences and @ref DebugUIHandlerReferences, and the BattleGameViewController script.
    public class BattleUiController : MonoBehaviour
    {
        /// <value>BattleGameViewController script back reference.</value>
        public BattleGameViewController GameViewController;

        /// @anchor UIHandlerReferences
        /// @name UI Handler script references
        /// Handler scripts control the visual functionality of the %UI prefabs and GameObjects.
        /// @{

        /// <summary>Reference to BattleUiAnnouncementHandler which handles showing the countdown.</summary>Part of @ref UIHandlerReferences.
        public BattleUiAnnouncementHandler AnnouncementHandler;

        /// <summary>Reference to BattleUiLoadScreenHandler which handles the loading screen before the game starts.</summary>Part of @ref UIHandlerReferences.
        public BattleUiLoadScreenHandler LoadScreenHandler;

        /// <summary>Reference to BattleUiGameOverHandler which handles showing the game over popup.</summary>Part of @ref UIHandlerReferences.
        public BattleUiGameOverHandler GameOverHandler;

        /// <summary>Reference to BattleUiTimerHandler which handles the game timer.</summary>Part of @ref UIHandlerReferences.
        public BattleUiTimerHandler TimerHandler;

        /// <summary>Reference to BattleUiDiamondsHandler which handles displaying the diamonds acquired during the game.</summary>Part of @ref UIHandlerReferences.
        public BattleUiDiamondsHandler DiamondsHandler;

        /// <summary>Reference to BattleUiGiveUpButtonHandler which handles the give up button.</summary>Part of @ref UIHandlerReferences.
        public BattleUiGiveUpButtonHandler GiveUpButtonHandler;

        /// <summary>Reference to BattleUiPlayerInfoHandler which handles the local player's and teammate's player info.</summary>Part of @ref UIHandlerReferences.
        public BattleUiPlayerInfoHandler PlayerInfoHandler;

        /// <summary>Reference to BattleUiJoystickHandler which handles the joysticks.</summary>Part of @ref UIHandlerReferences.
        public BattleUiJoystickHandler JoystickHandler;

        /// @}

        /// @anchor DebugUIHandlerReferences
        /// @name Debug UI Handler script references
        /// These handler scripts handle visual functionality for the debug %UI.
        /// @{

        /// <summary>Reference to BattleUiDebugStatsOverlayHandler which displays the character's stats.</summary>Part of @ref DebugUIHandlerReferences.
        public BattleUiDebugStatsOverlayHandler DebugStatsOverlayHandler;

        /// @}
    }
}
