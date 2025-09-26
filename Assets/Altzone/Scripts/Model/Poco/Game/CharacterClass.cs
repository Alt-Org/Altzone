using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Non-mutable <c>CharacterClass</c> that acts as base archetype for player created characters.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CharacterClass
    {
        public CharacterClassType Type;
        public string Name;
        public int Hp;
        public int Speed;
        public int CharacterSize;
        public int Attack;
        public int Defence;

        public CharacterClass(CharacterClassType type, int hp, int speed, int resistance, int attack, int defence)
        {
            Assert.IsTrue(hp >= 0);
            Assert.IsTrue(speed >= 0);
            Assert.IsTrue(resistance >= 0);
            Assert.IsTrue(attack >= 0);
            Assert.IsTrue(defence >= 0);
            Type = type;
            Name = GetClassName(type);
            Hp = hp;
            Speed = speed;
            CharacterSize = resistance;
            Attack = attack;
            Defence = defence;
        }

        public static CharacterClass CreateDummyFor(CharacterClassType type)
        {
            return new CharacterClass(type, 1, 1, 1, 1, 1);
        }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Name)}: {Name}" +
                   $", {nameof(Speed)}: {Speed}, {nameof(CharacterSize)}: {CharacterSize}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }

        public static string GetClassName(CharacterClassType type)
        {
            switch (type)
            {
                case CharacterClassType.Desensitizer:
                    return "Desensitizer";
                case CharacterClassType.Trickster:
                    return "Trickster";
                case CharacterClassType.Obedient:
                    return "Obedient";
                case CharacterClassType.Projector:
                    return "Projector";
                case CharacterClassType.Retroflector:
                    return "Retroflector";
                case CharacterClassType.Confluent:
                    return "Confluent";
                case CharacterClassType.Intellectualizer:
                    return "Intellectualizer";
                default:
                    return "Error";
            }
        }


        public static Color GetCharacterClassColor(CharacterClassType type)
        {
            Color color;

            switch (type)
            {
                case CharacterClassType.Desensitizer:
                    ColorUtility.TryParseHtmlString("#23B1B1", out color);
                    break;
                case CharacterClassType.Trickster:
                    ColorUtility.TryParseHtmlString("#278227", out color);
                    break;
                case CharacterClassType.Obedient:
                    ColorUtility.TryParseHtmlString("#DF8617", out color);
                    break;
                case CharacterClassType.Projector:
                    ColorUtility.TryParseHtmlString("#D5D51B", out color);
                    break;
                case CharacterClassType.Retroflector:
                    ColorUtility.TryParseHtmlString("#B13232", out color);
                    break;
                case CharacterClassType.Confluent:
                    ColorUtility.TryParseHtmlString("#891D89", out color);
                    break;
                case CharacterClassType.Intellectualizer:
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
