using System;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Lobby;

namespace MenuUi.Scripts.ReferenceSheets
{
    /// <summary>
    /// Reference sheet for game types. Used for altzone battle button.
    /// </summary>
    //[CreateAssetMenu(menuName = "ALT-Zone/GameTypeReference", fileName = "GameTypeReference")]
    public class GameTypeReference : ScriptableObject
    {
        [SerializeField] private List<GameTypeInfo> _info;

        /// <summary>
        /// Get all game type infos.
        /// </summary>
        /// <returns>List of GameTypeInfo objects.</returns>
        public List<GameTypeInfo> GetGameTypeInfos()
        {
            return _info;
        }
    }

    /// <summary>
    /// Serializable holder class for game type's info.
    /// </summary>
    [Serializable]
    public class GameTypeInfo
    {
        public Sprite Icon;
        public GameType gameType;
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
