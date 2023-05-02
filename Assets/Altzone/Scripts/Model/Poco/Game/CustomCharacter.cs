using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Player created custom 'game' character based on given <c>CharacterClass</c>.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CustomCharacter
    {
        public string Id;

        [ForeignKeyReference(nameof(CharacterClass))]
        public string CharacterClassId;

        public string UnityKey;
        public string Name;
        public int Speed;
        public int Resistance;
        public int Attack;
        public int Defence;

        public CustomCharacter(string id, string characterClassId, string unityKey, string name, int speed, int resistance, int attack, int defence)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(id));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(characterClassId));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(name));
            Id = id;
            CharacterClassId = characterClassId;
            UnityKey = unityKey;
            Name = name;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(CharacterClassId)}: {CharacterClassId}" +
                   $", {nameof(UnityKey)}: {UnityKey}, {nameof(Name)}: {Name}" +
                   $", {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }
    }
}