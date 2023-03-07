using System;
using System.Diagnostics.CodeAnalysis;

namespace GameServer.Scripts.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>Clan</c> entity.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanDto
    {
        /// <summary>
        /// Id is assigned by server.
        /// </summary>
        public int Id;
        
        /// <summary>
        /// Clan name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Clan short name aka tag.
        /// </summary>
        public string Tag;
        
        /// <summary>
        /// Clan's current coins value.
        /// </summary>
        public int GameCoins;
    }
}