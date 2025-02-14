using System;
using System.Collections.Generic;
using static MenuUI.Scripts.Lobby.GameTypeEnum;
using UnityEngine;

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
        public string Name;
        public GameType gameType;
    }
}
