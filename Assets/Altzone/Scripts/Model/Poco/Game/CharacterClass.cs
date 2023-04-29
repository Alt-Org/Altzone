using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Non-mutable <c>CharacterClass</c> that acts as base archetype for player created characters.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CharacterClass
    {
        public string CharacterClassId;
        public GestaltCycle GestaltCycle;
        public string Name;
        public int Speed;
        public int Resistance;
        public int Attack;
        public int Defence;

        public CharacterClass(string characterClassId, GestaltCycle gestaltCycle, string name, int speed, int resistance, int attack, int defence)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(characterClassId));
            Assert.AreNotEqual(GestaltCycle.None, gestaltCycle);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(name));
            Assert.IsTrue(speed >= 0);
            Assert.IsTrue(resistance >= 0);
            Assert.IsTrue(attack >= 0);
            Assert.IsTrue(defence >= 0);
            CharacterClassId = characterClassId;
            GestaltCycle = gestaltCycle;
            Name = name;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public static CharacterClass CreateDummyFor(string characterClassId)
        {
            return new CharacterClass(characterClassId, (GestaltCycle)1, "deleted", 1, 1, 1, 1);
        }

        public override string ToString()
        {
            return $"{nameof(CharacterClassId)}: {CharacterClassId}, {nameof(GestaltCycle)}: {GestaltCycle}, {nameof(Name)}: {Name}" +
                   $", {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }
    }
}