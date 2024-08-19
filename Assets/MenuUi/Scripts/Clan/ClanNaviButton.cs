using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;

public class ClanNaviButton : NaviButton
{
    [SerializeField] protected WindowDef _alternateNaviTarget;
    protected override void OnNaviButtonClick()
    {
        if (ServerManager.Instance.Clan == null)
        {
            Debug.Log($"naviTarget {_alternateNaviTarget} isCurrentPopOutWindow {_isCurrentPopOutWindow}", _alternateNaviTarget);
            var windowManager = WindowManager.Get();
            if (_isCurrentPopOutWindow)
            {
                windowManager.PopCurrentWindow();
            }
            // Check if navigation target window is already in window stack and we area actually going back to it via button.
            var windowCount = windowManager.WindowCount;
            if (windowCount > 1)
            {
                var targetIndex = windowManager.FindIndex(_alternateNaviTarget);
                if (targetIndex == 1)
                {
                    windowManager.GoBack();
                    return;
                }
                if (targetIndex > 1)
                {
                    windowManager.Unwind(_alternateNaviTarget);
                    windowManager.GoBack();
                    return;
                }
            }
            windowManager.ShowWindow(_alternateNaviTarget);
            return;
        }
        else base.OnNaviButtonClick();
    }
}
