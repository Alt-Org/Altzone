using System;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace MenuUi.Scripts.ReferenceSheets
{
    /// <summary>
    /// Reference sheet for game types. Used for altzone battle button.
    /// </summary>
    //[CreateAssetMenu(menuName = "ALT-Zone/GameTypeReference", fileName = "GameTypeReference")]
    public class GameTypeReference : ScriptableObject
    {
        private static GameTypeReference _instance;
        private static bool _hasInstance;

        public static GameTypeReference Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<GameTypeReference>(nameof(GameTypeReference));
                    _hasInstance = _instance != null;
                }
                return _instance;
            }
        }


        [SerializeField] private List<GameTypeInfo> _info;

        /// <summary>
        /// Get all game type infos.
        /// </summary>
        /// <returns>List of GameTypeInfo objects.</returns>
        public List<GameTypeInfo> GetGameTypeInfos()
        {
            return _info;
        }

        public int GetEnabledCount()
        {
            int count = 0;
            count = _info.FindAll(x => x.Enabled).Count;
            return count;
        }
    }

    /// <summary>
    /// Serializable holder class for game type's info.
    /// </summary>
    [Serializable]
    public class GameTypeInfo
    {
        public Sprite Icon;
        public Sprite Banner;
        public Sprite Background;
        public GameType gameType;
        public bool Enabled;
        public string FinnishName;
        public string FinnishDescription;
        public string EnglishName;
        public string EnglishDescription;

        public string Name
        {
            get
            {
                switch (SettingsCarrier.Instance.Language)
                {
                    case SettingsCarrier.LanguageType.Finnish:
                        return FinnishName;
                    case SettingsCarrier.LanguageType.English:
                        return EnglishName;
                    default:
                        return FinnishName;
                }
            }
        }

        public string Description
        {
            get
            {
                switch (SettingsCarrier.Instance.Language)
                {
                    case SettingsCarrier.LanguageType.Finnish:
                        return FinnishDescription;
                    case SettingsCarrier.LanguageType.English:
                        return EnglishDescription;
                    default:
                        return FinnishDescription;
                }
            }
        }
    }
}
