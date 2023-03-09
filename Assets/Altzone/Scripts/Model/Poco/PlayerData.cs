using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PlayerData
    {
        public int Id;
        public int ClanId;
        public int CurrentCustomCharacterId;
        public string Name;
        public int BackpackCapacity;
        public string UniqueIdentifier;

        public PlayerData(int id, int clanId, int currentCustomCharacterId, string name, int backpackCapacity, string uniqueIdentifier)
        {
            Id = id;
            ClanId = clanId;
            CurrentCustomCharacterId = currentCustomCharacterId;
            Name = name;
            BackpackCapacity = backpackCapacity;
            UniqueIdentifier = uniqueIdentifier;
        }

        public override string ToString()
        {
            return 
                $"{nameof(Id)}: {Id}, {nameof(ClanId)}: {ClanId}, {nameof(CurrentCustomCharacterId)}: {CurrentCustomCharacterId}" +
                $", {nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(UniqueIdentifier)}: {UniqueIdentifier}";
        }
    }
}