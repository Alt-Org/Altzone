using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UiProto.Scripts.Window
{
    //[CreateAssetMenu(menuName = "ALT-Zone/WindowNames")]
    public class WindowNames : ScriptableObject
    {
        public List<WindowIdDef> names;

        private static List<WindowId> cachedWindows;

        public static WindowIdDef getWindowIdDef(int windowId)
        {
            var windows = Resources.Load<WindowNames>(nameof(WindowNames));
            if (windows != null)
            {
                var window = windows.names.FirstOrDefault(x => x.windowId == windowId);
                if (window != null)
                {
                    return window;
                }
            }
            return new WindowIdDef();
        }

        public static List<WindowIdDef> loadWindowNameDefs()
        {
            var windows = Resources.Load<WindowNames>(nameof(WindowNames));
            if (windows != null)
            {
                return windows.names;
            }
            return new List<WindowIdDef>();
        }

        public static List<WindowId> getWindows()
        {
            if (cachedWindows == null)
            {
                var windows = Resources.Load<WindowNames>(nameof(WindowNames));
                cachedWindows = windows != null
                    ? windows.names.Select(x => new WindowId { windowId = x.windowId }).ToList()
                    : new List<WindowId>();
            }
            return cachedWindows;
        }
    }
}