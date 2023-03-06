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
        public string PlayerGuid;
        public int ClanId;
        public int CurrentCharacterModelId;
        public string Name;
        public int BackpackCapacity;
    }
}