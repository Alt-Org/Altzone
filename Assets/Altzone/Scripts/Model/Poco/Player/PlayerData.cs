using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;

namespace Altzone.Scripts.Model.Poco.Player
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
        
        public BattleCharacter BattleCharacter { get; private set; }
        public List<BattleCharacter> BattleCharacters { get; private set; }
        public List<CustomCharacter> CustomCharacters { get; private set; }

        public PlayerData(int id, int clanId, int currentCustomCharacterId, string name, int backpackCapacity, string uniqueIdentifier)
        {
            Id = id;
            ClanId = clanId;
            CurrentCustomCharacterId = currentCustomCharacterId;
            Name = name;
            BackpackCapacity = backpackCapacity;
            UniqueIdentifier = uniqueIdentifier;
        }

        internal void Patch(BattleCharacter battleCharacter, List<BattleCharacter> battleCharacters, List<CustomCharacter> customCharacters)
        {
            BattleCharacter = battleCharacter;
            BattleCharacters = new ReadOnlyCollection<BattleCharacter>(battleCharacters).ToList();
            CustomCharacters = new ReadOnlyCollection<CustomCharacter>(customCharacters).ToList();
        }
        
        public override string ToString()
        {
            return 
                $"{nameof(Id)}: {Id}, {nameof(ClanId)}: {ClanId}, {nameof(CurrentCustomCharacterId)}: {CurrentCustomCharacterId}" +
                $", {nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(UniqueIdentifier)}: {UniqueIdentifier}";
        }
    }
}