using QuantumUser.Scripts.UI.Views;
using UnityEngine;

namespace QuantumUser.Scripts.UI
{
    /**
     *  Base class that handles game ui
     *  Should take most of its commands from the GameViewController, which in turn monitors the game session state on the quantum side
     */
    public class GameUiController : MonoBehaviour
    {
        // View Scripts
        public GameUiAnnouncementHandler AnnouncementHandler;
        public GameUiGameOverHandler GameOverHandler;
        public GameUiTimerHandler TimerHandler;
        public GameUiDiamondsHandler DiamondsHandler;
        public GameUiGiveUpButtonHandler GiveUpButtonHandler;
        public GameUiPlayerInfoHandler PlayerInfoHandler;

        // Debug view scripts
        public GameUiDebugStatsOverlay DebugStatsOverlay;
    }
}

