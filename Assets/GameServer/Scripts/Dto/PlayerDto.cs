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
        /// <summary>
        /// Id is assigned by server.
        /// </summary>
        public int Id;
        
        /// <summary>
        /// PlayerGuid (Globally Unique Identifier) is used to identify player from client without knowing the id.
        /// </summary>
        /// <remarks>
        /// This is assigned by the client when it creates <c>PlayerDto</c> when the game is started first time.
        /// </remarks>
        public string PlayerGuid;
        
        /// <summary>
        /// Player's clan id or zero.
        /// </summary>
        public int ClanId;
        
        /// <summary>
        /// Player's currently selected character model id.
        /// </summary>
        public int CurrentCharacterModelId;
        
        /// <summary>
        /// Player name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Player's backpack capacity in kilos (kg).
        /// </summary>
        public int BackpackCapacity;
    }
}