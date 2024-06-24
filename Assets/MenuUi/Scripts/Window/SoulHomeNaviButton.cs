using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    public class SoulHomeNaviButton : NaviButton
    {
        [SerializeField] private GameObject _popup;
        protected override void OnNaviButtonClick()
        {
            if (ServerManager.Instance.Clan == null)
            {
                _popup = transform.root.Find("MainMenuViewSwipe/Scroll View/Viewport/UIPanel/Interactables/InfoPopup").gameObject; // This needs better solution. Probably this should be held by a carrier script in MainManuViewSwipe.
                if (_popup != null) _popup.GetComponent<PopupController>().ActivatePopUp("Sinun pitää liittyä klaaniin päästäksesi sielunkotiin.");
                return;
            }
            Debug.Log($"naviTarget {_naviTarget} isCurrentPopOutWindow {_isCurrentPopOutWindow}", _naviTarget);
            var windowManager = WindowManager.Get();
            if (_isCurrentPopOutWindow)
            {
                windowManager.PopCurrentWindow();
            }
            // Check if navigation target window is already in window stack and we area actually going back to it via button.
            var windowCount = windowManager.WindowCount;
            if (windowCount > 1)
            {
                var targetIndex = windowManager.FindIndex(_naviTarget);
                if (targetIndex == 1)
                {
                    windowManager.GoBack();
                    return;
                }
                if (targetIndex > 1)
                {
                    windowManager.Unwind(_naviTarget);
                    windowManager.GoBack();
                    return;
                }
            }
            windowManager.ShowWindow(_naviTarget);
        }
    }
}
