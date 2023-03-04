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
        public int Id;
        public string Name;
        public string Tag;
        public int GameCoins;
    }
}