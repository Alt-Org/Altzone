using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Player created custom 'game' character based on given <c>CharacterClass</c>.
    /// </summary>
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CustomCharacter
    {
        [PrimaryKey] public string Id;
        [ForeignKey(nameof(CharacterClass)), Mandatory] public string CharacterClassId;

        /// <summary>
        /// This can be used for example to load UNITY assets by name for UI at runtime.
        /// </summary>
        [Optional] public int UnityKey = -1;

        [Mandatory] public string Name;
        public int Speed;
        public int Resistance;
        public int Attack;
        public int Defence;

        public CustomCharacter(string id, string characterClassId, int unityKey, string name, int speed, int resistance, int attack, int defence)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(characterClassId.IsMandatory());
            Assert.IsTrue(name.IsMandatory());
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
