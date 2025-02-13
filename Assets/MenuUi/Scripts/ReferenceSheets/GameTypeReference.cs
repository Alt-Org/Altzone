using System;
using System.Collections.Generic;
using static MenuUI.Scripts.Lobby.InLobby.InLobbyController;
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
        /// Get game type's info.
        /// </summary>
        /// <param name="gameType">The game type which's info to get.</param>
        /// <returns>GameTypeInfo object.</returns>
        public GameTypeInfo GetGameTypeInfo(GameType gameType)
        {
            foreach (GameTypeInfo info in _info)
            {
                if (info.gameType == gameType)
                {
                    return info;
                }
            }
            return null;
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
