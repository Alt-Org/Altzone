using System;
using UnityEngine;

namespace UiProto.Scripts.Window
{
    [Serializable]
    public class WindowInstance
    {
        public WindowId window;
        public GameObject windowTemplate;

        public string windowTemplateName => windowTemplate != null ? windowTemplate.name : "";

        public override string ToString()
        {
            var templateInfo = windowTemplate != null ? $"{windowTemplate.name} ({windowTemplate.scene.handle})" : "";
            return $"{nameof(window)}: {window}, {nameof(windowTemplate)}: {templateInfo}";
        }
    }
}