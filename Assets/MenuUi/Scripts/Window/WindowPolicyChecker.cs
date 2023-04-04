using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    public class WindowPolicyChecker : MonoBehaviour
    {
        private static readonly string[] KnownFontNames =
        {
            "WorkSans",
        };

        private void Awake()
        {
            CheckCanvas(FindObjectOfType<Canvas>());
            enabled = false;
        }

        private static void CheckCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }
            var components = new HashSet<Component>();
            foreach (var text in canvas.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true))
            {
                CheckFontName(components, text, text.font.name);
            }
            foreach (var text in canvas.GetComponentsInChildren<Text>(includeInactive: true))
            {
                CheckFontName(components, text, text.font.name);
            }
            foreach (var text in canvas.GetComponentsInChildren<TMP_Text>(includeInactive: true))
            {
                CheckFontName(components, text, text.font.name);
            }
        }

        private static void CheckFontName(HashSet<Component> components, Component component, string fontName)
        {
            if (components.Contains(component))
            {
                return;
            }
            components.Add(component);
            var isValidTextType = component is TextMeshProUGUI;
            var isKnownFont = false;
            foreach (var knownFontName in KnownFontNames)
            {
                if (fontName.Contains(knownFontName))
                {
                    isKnownFont = true;
                    break;
                }
            }
            if (isValidTextType && isKnownFont)
            {
                return;
            }
            var componentText = isValidTextType ? component.name
                : component is TMP_Text ? $"{RichText.Yellow(component.name)} <i>text type is deprecated</i>"
                : $"{RichText.Yellow(component.name)} <i>text type is old/legacy</i>";
            var fontText = isKnownFont ? fontName : $"{RichText.Yellow(fontName)} <i>should not use this font</i>";
            var marker = "<color=orange>*</color>";
            if (!isKnownFont)
            {
                UnityEngine.Debug.LogError($"{fontText} in {componentText} {marker}", component);
                return;
            }
            UnityEngine.Debug.LogWarning($"{fontText} in {componentText} {marker}", component);
        }
    }
}