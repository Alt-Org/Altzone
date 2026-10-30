using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Prg.Localization
{
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class LocaleData
    {
        // ReSharper disable once NotAccessedField.Global
        [AllowNesting, ReadOnly] public string _name;
        public SystemLanguage SystemLanguage;
        public string LocaleCode;
        public Sprite Flag;

        public override string ToString() => $"{SystemLanguage} ({LocaleCode})";
    }

    [CreateAssetMenu(menuName = "Prg/Game/" + nameof(LocaleSettings), fileName = nameof(LocaleSettings))]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class LocaleSettings : ScriptableObject
    {
        public const string DefaultLocaleCode = "en";

        #region UNITY Singleton Pattern

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private static LocaleSettings _instance;

        public static LocaleSettings Get()
        {
            return _instance ??= Resources.Load<LocaleSettings>(nameof(LocaleSettings));
        }

        #endregion

        #region PlayerPrefs getters and setters

        public static string CurrentLocale
        {
            get => PlayerPrefs.GetString(nameof(CurrentLocale), DefaultLocaleCode);
            set
            {
                var localeData = Get().GetLocaleFor(value);
                PlayerPrefs.SetString(nameof(CurrentLocale), localeData.LocaleCode);
            }
        }

        public static LocaleData CurrentLocaleData => Get().GetLocaleFor(CurrentLocale);

        #endregion

        private const string Info1 = "Available Locales, first item is default locale if specific locale is not found in here";
        private const string Info2 = "Use MissingKey Settings in production builds";
        private const string Info3 = "MissingKey Settings for development builds";

        [InfoBox(Info1)] public LocaleData[] Locales;
        [ReadOnly] public LocaleData DefaultLocale;

        [Header("MissingKey Settings"), InfoBox(Info2)] public bool IsForceInProduction;
        [InfoBox(Info3)] public bool IsShowMissingKey;
        public bool IsShowMissingKeyColor;
        public Color MissingKeyColor = Color.red;

        private void OnValidate()
        {
            if (Locales == null)
            {
                return;
            }
            foreach (var locale in Locales)
            {
                if (locale == null)
                {
                    continue;
                }
                locale._name = locale.LocaleCode;
            }
            DefaultLocale ??= Locales[0];
            if (DefaultLocale.LocaleCode != DefaultLocaleCode)
            {
                DefaultLocale = Locales.FirstOrDefault(x => x.LocaleCode == DefaultLocale.LocaleCode) ?? Locales[0];
            }
        }

        private LocaleData GetLocaleFor(string localeCode)
        {
            MyAssert.IsNotNull(Locales, "Locales array is required", this);
            var index = Array.FindIndex(Locales, x => x.LocaleCode == localeCode);
            return index == -1
                ? DefaultLocale
                : Locales[index];
        }
    }
}
