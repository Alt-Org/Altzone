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

        // Views - DISABLED in scene by default
        public GameObject GameOverView;
        public GameObject AnnouncerView;


        //View Scripts
        public UIGameAnnouncementHandler uiGameAnnouncementHandler;
        public UIGameOverHandler uiGameOverHandler;

    }
}

