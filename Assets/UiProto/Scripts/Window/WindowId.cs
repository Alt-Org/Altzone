using System;

namespace UiProto.Scripts.Window
{
    [Serializable]
    public class WindowId // Read only 1 liner for Editor config popups
    {
        public int windowId;
        public string windowName => windowIdDef.windowName;

        [NonSerialized]
        private WindowIdDef _def;

        private WindowIdDef windowIdDef => _def ?? (_def = WindowNames.getWindowIdDef(windowId));

        public override string ToString()
        {
            return windowIdDef.ToString();
        }
    }

    [Serializable]
    public class WindowIdDef // Full editing capabilities
    {
        public string windowName;
        public int windowId;

        public override string ToString()
        {
            return $"{windowName} [{windowId}]";
        }
    }
}