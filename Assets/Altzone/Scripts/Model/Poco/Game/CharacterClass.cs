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
        public int CharacterSize;
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
            CharacterSize = resistance;
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
                   $", {nameof(Speed)}: {Speed}, {nameof(CharacterSize)}: {CharacterSize}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
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
            Color color;

            switch (id)
            {
                case CharacterClassID.Desensitizer:
                    ColorUtility.TryParseHtmlString("#23B1B1", out color);
                    break;
                case CharacterClassID.Trickster:
                    ColorUtility.TryParseHtmlString("#278227", out color);
                    break;
                case CharacterClassID.Obedient:
                    ColorUtility.TryParseHtmlString("#DF8617", out color);
                    break;
                case CharacterClassID.Projector:
                    ColorUtility.TryParseHtmlString("#D5D51B", out color);
                    break;
                case CharacterClassID.Retroflector:
                    ColorUtility.TryParseHtmlString("#B13232", out color);
                    break;
                case CharacterClassID.Confluent:
                    ColorUtility.TryParseHtmlString("#891D89", out color);
                    break;
                case CharacterClassID.Intellectualizer:
                    ColorUtility.TryParseHtmlString("#522295", out color);
                    break;
                default:
                    color = Color.gray;
                    break;
            }

            return color;
        }
    }
}
