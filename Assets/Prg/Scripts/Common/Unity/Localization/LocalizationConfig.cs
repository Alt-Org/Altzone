using UnityEngine;

namespace Prg.Scripts.Common.Unity.Localization
{
    [CreateAssetMenu(menuName = "ALT-Zone/LocalizationConfig", fileName = "LocalizationConfig")]
    public class LocalizationConfig : ScriptableObject
    {
        [SerializeField] private TextAsset _translationsTsvFile;
        [SerializeField] private TextAsset _languagesBinFile;

        public TextAsset TranslationsTsvFile => _translationsTsvFile;
        public TextAsset LanguagesBinFile => _languagesBinFile;
    }
}