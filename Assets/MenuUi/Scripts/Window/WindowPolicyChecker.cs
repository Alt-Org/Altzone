using System.Collections.Generic;
using MenuUi.Scripts.Window.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    public class WindowPolicyChecker : MonoBehaviour
    {
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
            var allowedFonts = Resources.Load<AllowedFonts>(nameof(AllowedFonts));
            if (allowedFonts == null)
            {
                return;
            }
            var knownFontNames = new List<string>();
            if (allowedFonts._tmpFonts != null)
            {
                foreach (var font in allowedFonts._tmpFonts)
                {
                    knownFontNames.Add(font.name);
                }
            }
            if (allowedFonts._fonts != null)
            {
                foreach (var font in allowedFonts._fonts)
                {
                    knownFontNames.Add(font.name);
                }
            }
            var components = new HashSet<Component>();
            foreach (var text in canvas.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true))
            {
                CheckFontName(components, text, knownFontNames, text.font.name);
            }
            foreach (var text in canvas.GetComponentsInChildren<Text>(includeInactive: true))
            {
                CheckFontName(components, text, knownFontNames, text.font.name);
            }
            foreach (var text in canvas.GetComponentsInChildren<TMP_Text>(includeInactive: true))
            {
                CheckFontName(components, text, knownFontNames, text.font.name);
            }
        }

        private static void CheckFontName(HashSet<Component> components, Component component, List<string> knownFontNames, string fontName)
        {
            if (components.Contains(component))
            {
                return;
            }
            components.Add(component);
            var isValidTextType = component is TextMeshProUGUI;
            var isKnownFont = false;
            foreach (var knownFontName in knownFontNames)
            {
                if (fontName != knownFontName)
                {
                    continue;
                }
                isKnownFont = true;
                break;
            }
            if (isValidTextType && isKnownFont)
            {
                // Nothing to complain.
                return;
            }
            var componentText = isValidTextType ? component.name
                : component is TMP_Text ? $"{RichText.Yellow(component.name)} <i>text type is deprecated</i>"
                : $"{RichText.Yellow(component.name)} <i>text type is old/legacy</i>";
            var fontText = isKnownFont ? fontName : $"{RichText.Yellow(fontName)} <i>should not use this font</i>";
            var marker = "<color=orange>*</color>";
            if (isKnownFont)
            {
                // Just warning when Text component is not TextMeshProUGUI
                UnityEngine.Debug.LogWarning($"{fontText} in {componentText} {marker}", component);
                return;
            }
            UnityEngine.Debug.LogError($"{fontText} in {componentText} {marker}", component);
        }
    }
}