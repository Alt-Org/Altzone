using UnityEngine;

using Battle.View.Game;

namespace Battle.View.UI
{
    /**
     *  Base class that handles battle ui
     *  Should take most of its commands from the BattleGameViewController, which in turn monitors the game session state on the quantum side
     */
    public class BattleUiController : MonoBehaviour
    {
        // View controller script back reference
        public BattleGameViewController GameViewController;

        // UI Handler Scripts
        public BattleUiAnnouncementHandler AnnouncementHandler;
        public BattleUiGameOverHandler GameOverHandler;
        public BattleUiTimerHandler TimerHandler;
        public BattleUiDiamondsHandler DiamondsHandler;
        public BattleUiGiveUpButtonHandler GiveUpButtonHandler;
        public BattleUiPlayerInfoHandler PlayerInfoHandler;

        // UI Debug Handler scripts
        public BattleUiDebugStatsOverlayHandler DebugStatsOverlayHandler;
    }
}
