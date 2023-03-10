using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco
{
    /// <summary>
    /// Player created custom 'game' character based on given <c>CharacterClass</c>.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CustomCharacter
    {
        public int Id;
        public int CharacterClassId;
        public string PlayerPrefabKey;
        public string Name;
        public int Speed;
        public int Resistance;
        public int Attack;
        public int Defence;

        public CustomCharacter(int id, int characterClassId, string playerPrefabKey, string name, int speed, int resistance, int attack, int defence)
        {
            Id = id;
            CharacterClassId = characterClassId;
            PlayerPrefabKey = playerPrefabKey;
            Name = name;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(CharacterClassId)}: {CharacterClassId}" +
                   $", {nameof(PlayerPrefabKey)}: {PlayerPrefabKey}, {nameof(Name)}: {Name}" +
                   $", {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }
    }
}