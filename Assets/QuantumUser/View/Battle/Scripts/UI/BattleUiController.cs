/// @file BattleUiController.cs
/// <summary>
/// Has a base class BattleUiController which controls the %Battle %UI.
/// </summary>
///
/// This script:<br/>
/// Holds references to all of the @ref UIHandlers and @ref DebugUIHandlers scripts, and the BattleGameViewController script.

using UnityEngine;

using Battle.View.Game;

namespace Battle.View.UI
{
    /// <summary>
    /// Base class that handles %Battle %UI.<br/>
    /// BattleGameViewController accesses the @ref UIHandlers scripts through this class.
    /// </summary>
    ///
    /// Holds references to all of the @ref UIHandlers and @ref DebugUIHandlers scripts, and the BattleGameViewController script.
    public class BattleUiController : MonoBehaviour
    {
        /// <value>BattleGameViewController script back reference.</value>
        public BattleGameViewController GameViewController;

        /// @anchor UIHandlers
        /// @name UI Handler script references
        /// Handler scripts control the visual functionality of the %UI prefabs and GameObjects.
        /// @{

        /// <value>Reference to BattleUiAnnouncementHandler which handles showing the countdown.</value>
        public BattleUiAnnouncementHandler AnnouncementHandler;

        /// <value>Reference to BattleUiGameOverHandler which handles showing the game over popup.</value>
        public BattleUiGameOverHandler GameOverHandler;

        /// <value>Reference to BattleUiTimerHandler which handles the game timer.</value>
        public BattleUiTimerHandler TimerHandler;

        /// <value>Reference to BattleUiDiamondsHandler which handles displaying the diamonds acquired during the game.</value>
        public BattleUiDiamondsHandler DiamondsHandler;

        /// <value>Reference to BattleUiGiveUpButtonHandler which handles the give up button.</value>
        public BattleUiGiveUpButtonHandler GiveUpButtonHandler;

        /// <value>Reference to BattleUiPlayerInfoHandler which handles the local player's and teammate's player info.</value>
        public BattleUiPlayerInfoHandler PlayerInfoHandler;

        /// @}

        /// @anchor DebugUIHandlers
        /// @name Debug UI Handler script references
        /// These handler scripts handle visual functionality for the debug %UI.
        /// @{

        /// <value>Reference to BattleUiDebugStatsOverlayHandler which displays the character's stats.</value>
        public BattleUiDebugStatsOverlayHandler DebugStatsOverlayHandler;

        /// @}
    }
}
