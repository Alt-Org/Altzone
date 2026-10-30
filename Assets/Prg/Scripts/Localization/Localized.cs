using System.Linq;
using NaughtyAttributes;
using Prg.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Localization
{
    public enum LocalizedComponentType
    {
        Unknown,
        Text,
        Button,
        Toggle,
    }

    /// <summary>
    /// Helper interface for those components that needs to be manually notified when locale changes.
    /// </summary>
    public interface ILocalizable
    {
        void Localize();
    }

    /// <summary>
    /// Localized component to hold localization key and default value for text (if key not found).
    /// </summary>
    public class Localized : MonoBehaviour
    {
        /// <summary>
        /// NoLocalizeTag is used to skip localization and must be set externally to take effect.<br />
        /// Recommended value for it is: &quot;NoLocalize&quot;.<br />
        /// It is left intentionally un-initialized!
        /// </summary>
        public static string NoLocalizeTag;

        private const int MaxKeyLenth = 60;
        private const string EmptyMarkerValue = "EMPTY_TEXT";
        private const char PathSeparator = '/';
        private const char KeySeparator = '.';
        private const char CommentEndMarker = '#';
        private const string PrefabCanvasEnvironment = "Canvas (Environment)/";
        private static readonly string[] _ignoreComponents =
        {
            ".Canvas.",
            ".Panel.",
        };

        [Header("Settings")]
        [Label("Manual Update with Code"), AllowNesting] public bool _isManualUpdate;
        [Label("Localization Key"), AllowNesting] public string _key;
        [ReadOnly, AllowNesting] public string _defaultText;

        [Header("Text Component"), ReadOnly, AllowNesting] public LocalizedComponentType _componentType;
        [ReadOnly, AllowNesting] public TMP_Text _textMeshPro;

        public string SafeText => _textMeshPro != null ? _textMeshPro.text : EmptyMarkerValue;

        private bool _hasTextComponent;

        private void Awake()
        {
            _hasTextComponent = _textMeshPro != null;
            if (!_hasTextComponent)
            {
                Debug.LogError($"textMeshPro is NULL for {_componentType} [{_key}]: {this.GetFullPath()}", this);
                return;
            }
            if (_componentType == LocalizedComponentType.Button)
            {
                // Force button text to be one liner!
                _textMeshPro.enableWordWrapping = false;
                _textMeshPro.overflowMode = TextOverflowModes.Overflow;
            }
            if (_key.EndsWith(CommentEndMarker))
            {
                Debug.LogError($"[{_key}]: {_textMeshPro.text} | key ends with {CommentEndMarker}");
            }
            else
            {
                Debug.Log($"[{_key}]: {_textMeshPro.text}");
            }
            MyAssert.AreNotEqual(LocalizedComponentType.Unknown, _componentType, "valid componentType is required", this);
        }

        public void SetText(string text)
        {
            if (!_hasTextComponent)
            {
                return;
            }
            Debug.Log($"[{_key}]: {_textMeshPro.text} <- {text}");
            _textMeshPro.text = text;
        }

        public void SetColor(Color color)
        {
            if (!_hasTextComponent)
            {
                return;
            }
            _textMeshPro.color = color;
        }

        private void OnEnable()
        {
            this.Localize();
#if UNITY_EDITOR
            Assert.IsFalse(string.IsNullOrEmpty(NoLocalizeTag));
            if (gameObject.CompareTag(NoLocalizeTag))
            {
                SetColor(Localizer.MissingKeyColor);
                Debug.LogError($"NoLocalize Tag in: {this.GetFullPath()}", this);
            }
#endif
        }

        public override string ToString()
        {
            return $"key={_key}, text={SanitizeText(_defaultText)}";
        }

        public static string SanitizeText(string text)
        {
            text = RegexUtil.RemoveAllTags(text);
            text = RegexUtil.RemoveAllEmptyLines(text);
            text = RegexUtil.ReplaceCrLf(text, ". ");
            if (text.Length == 0)
            {
                text = EmptyMarkerValue;
            }
            return text.Length > 50 ? text[..50] : text;
        }

#if UNITY_EDITOR
        public static string KeyFrom(Component component)
        {
            var path = component.GetFullPath();
            if (path.StartsWith(PrefabCanvasEnvironment))
            {
                path = path.Remove(0, PrefabCanvasEnvironment.Length);
            }
            foreach (var ignoreComponent in _ignoreComponents)
            {
                path = path.Replace(ignoreComponent, ".");
            }
            while (path.Length > MaxKeyLenth)
            {
                var tokens = path.Split(PathSeparator).ToList();
                tokens.RemoveAt(0);
                path = string.Join(PathSeparator.ToString(), tokens);
            }
            if (path.EndsWith(CommentEndMarker))
            {
                path = path[..^1];
            }
            return path.Replace(PathSeparator, KeySeparator);
        }
#endif
    }
}
