using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Non-mutable <c>CharacterClass</c> that acts as base archetype for player created characters.
    /// </summary>
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CharacterClass
    {
        [PrimaryKey] public CharacterClassID Id;
        [Unique] public string Name;
        public int Hp;
        public int Speed;
        public int Resistance;
        public int Attack;
        public int Defence;

        public CharacterClass(CharacterClassID id, int hp, int speed, int resistance, int attack, int defence)
        {
            //Assert.AreNotEqual(CharacterClassID.None, id);
            Assert.IsTrue(hp >= 0);
            Assert.IsTrue(speed >= 0);
            Assert.IsTrue(resistance >= 0);
            Assert.IsTrue(attack >= 0);
            Assert.IsTrue(defence >= 0);
            Id = id;
            Name = GetClassName(id);
            Hp = hp;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public static CharacterClass CreateDummyFor(CharacterClassID id)
        {
            return new CharacterClass(id, 1, 1, 1, 1, 1);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}" +
                   $", {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }

        public static string GetClassName(CharacterClassID id)
        {
            switch (id)
            {
                case CharacterClassID.Desensitizer:
                    return "Desensitizer";
                case CharacterClassID.Trickster:
                    return "Trickster";
                case CharacterClassID.Obedient:
                    return "Obedient";
                case CharacterClassID.Projector:
                    return "Projector";
                case CharacterClassID.Retroflector:
                    return "Retroflector";
                case CharacterClassID.Confluent:
                    return "Confluent";
                case CharacterClassID.Intellectualizer:
                    return "Intellectualizer";
                default:
                    return "Error";
            }
        }


        public static Color GetCharacterClassColor(CharacterClassID id)
        {
            switch (id)
            {
                case CharacterClassID.Desensitizer:
                    return new Color(0.68f, 0.84f, 0.9f, 1);
                case CharacterClassID.Trickster:
                    return Color.green;
                case CharacterClassID.Obedient:
                    return new Color(1f, 0.64f, 0, 1);
                case CharacterClassID.Projector:
                    return Color.yellow;
                case CharacterClassID.Retroflector:
                    return Color.red;
                case CharacterClassID.Confluent:
                    return new Color(0.5f, 0, 0.5f, 1);
                case CharacterClassID.Intellectualizer:
                    return Color.blue;
                default:
                    return Color.gray;
            }
        }
    }
}
