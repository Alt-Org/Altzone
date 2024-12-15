using QuantumUser.Scripts.UI.Views;
using UnityEngine;

namespace QuantumUser.Scripts.UI
{
    /**
 *  Base class that handles any game views shown or hidden during that "battle stage" should take most of its commands from the BattleUIController, which in turn monitors the game session state on the quantum side
 */
    public class GameViewController : MonoBehaviour
    {

        // Views - DISABLED in scene by default
        public GameObject GameOverView;
        public GameObject AnnouncerView;


        //View Scripts
        public UIGameAnnouncementHandler uiGameAnnouncementHandler;
        public UIGameOverHandler uiGameOverHandler;

    }
}

