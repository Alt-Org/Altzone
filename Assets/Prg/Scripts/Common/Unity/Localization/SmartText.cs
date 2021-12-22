using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.Localization
{
    /// <summary>
    /// Class for UI <c>Text</c> component localization.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class SmartText : MonoBehaviour
    {
        [SerializeField] protected string _localizationKey;

        [Header("Live Data"), SerializeField] protected Text _text;

        public string LocalizationKey
        {
            get => _localizationKey;
            set => _localizationKey = value;
        }

        public string ComponentName => GetComponentName();

        private void OnEnable()
        {
            Localize();
        }

        public void Localize()
        {
            var localizedText = Localizer.Localize(_localizationKey);
            _text = GetComponent<Text>();
            _text.text = localizedText;
            Localizer.LocalizerHelper.TrackWords(this, _localizationKey, localizedText);
        }

        private string GetComponentName()
        {
            string[] excludedNames =
            {
                "panel",
                "window",
                "canvas",
                "environment",
            };
            // For example buttons have text component with name "Text" that is redundant.
            const string trailingText = "/text";

            // Get path without top most gameObject(s) (filtered by name).
            var followTransform = transform;
            var builder = new StringBuilder(followTransform.name);
            for (;;)
            {
                followTransform = followTransform.parent;
                if (followTransform == null)
                {
                    break;
                }
                var transformName = followTransform.name;
                var lowerName = transformName.ToLower();
                var found = excludedNames.FirstOrDefault(x => lowerName.Contains(x));
                if (found != null)
                {
                    continue;
                }
                builder.Insert(0, "/").Insert(0, transformName);
            }
            var path = builder.ToString();
            if (path.ToLower().EndsWith(trailingText))
            {
                var pos1 = path.IndexOf('/');
                var pos2 = path.LastIndexOf('/');
                if (pos1 != pos2 && pos2 != -1)
                {
                    path = path.Substring(0, path.Length - trailingText.Length);
                }
            }
            return path;
        }
    }
}