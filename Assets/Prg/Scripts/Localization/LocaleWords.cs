using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Prg.Localization
{
    [Serializable]
    public class ListItems
    {
#if UNITY_EDITOR
        public bool ShowNameIf() => false;
#endif

        [SerializeField, AllowNesting, ShowIf("ShowNameIf")] private string _name;
        [SerializeField] private List<string> _items;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public List<string> Items
        {
            get => _items;
            set => _items = value;
        }
    }

    [CreateAssetMenu(menuName = "Prg/Game/" + nameof(LocaleWords), fileName = nameof(LocaleWords))]
    public class LocaleWords : ScriptableObject
    {
        #region UNITY Singleton Pattern

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private static LocaleWords _instance;

        public static LocaleWords Get()
        {
            return _instance ??= Resources.Load<LocaleWords>(nameof(LocaleWords));
        }

        #endregion

        [Header("Localized Word Dictionaries")]
        [Label("Localization Keys"),ReadOnly, SerializeField] private ListItems _keys;
        [Label("Localized Words by Locale"),ReadOnly, SerializeField] private List<ListItems> _words;

        #region Import/Export suppor for Editor

#if UNITY_EDITOR
        public ListItems Keys
        {
            get => _keys;
            set => _keys = value;
        }
        public List<ListItems> Words
        {
            get => _words;
            set => _words = value;
        }
#endif

        #endregion

        public Dictionary<string, Dictionary<string, string>> LoadWords()
        {
            // Convert asset back to C# Dictionary of Dictionaries.
            var localeWords = new Dictionary<string, Dictionary<string, string>>();

            var keys = _keys.Items;
            var keyCount = keys.Count;
            var localeCodes = _words.Select(x => x.Name).ToList();
            foreach (var localeCode in localeCodes)
            {
                var dictionary = new Dictionary<string, string>();
                var words = _words.Find(x => x.Name == localeCode).Items;
                for (var i = 0; i < keyCount; ++i)
                {
                    var key = keys[i];
                    var word = words[i];
                    dictionary.Add(key, word);
                }
                localeWords.Add(localeCode, dictionary);
            }
            return localeWords;
        }
    }
}
