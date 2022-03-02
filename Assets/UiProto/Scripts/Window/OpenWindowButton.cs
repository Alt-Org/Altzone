using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UiProto.Scripts.Window
{
    [RequireComponent(typeof(Button))]
    public class OpenWindowButton : MonoBehaviour
    {
        public WindowId window;
        public LevelId levelId;

        private void Start()
        {
            var button = GetComponent<Button>();
            var currentWindow = WindowManager.Get().CurrentWindow.windowId;
            if (currentWindow.windowId == window.windowId)
            {
                button.interactable = false;
                return;
            }
            button.onClick.AddListener(() =>
            {
                if (window.windowId == 0)
                {
                    Debug.LogError($"{getButtonInfo(button)} <color=red>MUST BE CONFIGURED</color>", this);
                    return;
                }
                try
                {
                    WindowManager.Get().ShowWindow(window, levelId);
                }
                catch (Exception)
                {
                    Debug.LogError($"{getButtonInfo(button)} <color=red>CONFIG ERROR</color>", this);
                    throw;
                }
            });
        }

        private static string getButtonInfo(Button button)
        {
            var builder = new StringBuilder("Button ")
                .Append("<color=yellow>").Append(button.name).Append("</color>")
                .Append(" click");
            var text = button.GetComponentInChildren<Text>();
            if (text != null && !string.IsNullOrWhiteSpace(text.text))
            {
                builder.Append(", LABEL:").Append(text.text);
            }
            return builder.ToString();
        }
    }
}