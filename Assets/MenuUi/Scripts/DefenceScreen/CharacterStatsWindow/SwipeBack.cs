using SwipeHandler = MenuUi.Scripts.AvatarEditor.SwipeHandler;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using MenuUi.Scripts.AvatarEditor;
using MenuUi.Scripts.Window;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Is used to handle swiping back from character stats window.
    /// </summary>
    public class SwipeBack : MonoBehaviour
    {
        [SerializeField] SwipeUI _swipeUi;

        private void Awake()
        {
            SwipeHandler.OnSwipe += HandleSwipe;
        }


        private void OnDestroy()
        {
            SwipeHandler.OnSwipe -= HandleSwipe;
        }


        private void HandleSwipe(SwipeDirection direction, Vector2 swipeStartPoint, Vector2 swipeEndPoint)
        {
            if (_swipeUi.CurrentPage == 0 && direction == SwipeDirection.Right && _swipeUi.ScrollbarValue < 0.01f)
            {
                WindowManager.Get().GoBack();
            }
        }
    }
}
