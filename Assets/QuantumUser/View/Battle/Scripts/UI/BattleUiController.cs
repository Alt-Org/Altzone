using UnityEngine;

namespace Battle.View.UI
{
    /**
     *  Base class that handles battle ui
     *  Should take most of its commands from the BattleGameViewController, which in turn monitors the game session state on the quantum side
     */
    public class BattleUiController : MonoBehaviour
    {
        // UI Handler Scripts
        public BattleUiAnnouncementHandler AnnouncementHandler;
        public BattleUiGameOverHandler GameOverHandler;

        // UI Debug Handler scripts
        public BattleUiDebugStatsOverlayHandler DebugStatsOverlayHandler;
    }
}

