using System;
using System.Diagnostics.CodeAnalysis;

namespace GameServer.Scripts.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>Player</c> entity.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PlayerDto
    {
        public int Id;
        public int ClanId;
        public string Name;
        public int BackpackCapacity;
    }
}