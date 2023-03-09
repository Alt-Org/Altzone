using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PlayerData
    {
        public int Id;
        public int ClanId;
        public int CurrentCharacterModelId;
        public string Name;
        public int BackpackCapacity;
        public string UniqueIdentifier;

        public PlayerData(int id, int clanId, int currentCharacterModelId, string name, int backpackCapacity, string uniqueIdentifier)
        {
            Id = id;
            ClanId = clanId;
            CurrentCharacterModelId = currentCharacterModelId;
            Name = name;
            BackpackCapacity = backpackCapacity;
            UniqueIdentifier = uniqueIdentifier;
        }
    }
}