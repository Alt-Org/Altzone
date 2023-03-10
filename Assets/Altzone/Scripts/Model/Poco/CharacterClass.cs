using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco
{
    /// <summary>
    /// Non-mutable <c>CharacterClass</c> that acts as base archetype for player created characters.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CharacterClass
    {
        public int Id;
        public string Name;
        public Defence MainDefence;
        public int Speed;
        public int Resistance;
        public int Attack;
        public int Defence;

        public CharacterClass(int id, string name, Defence mainDefence, int speed, int resistance, int attack, int defence)
        {
            Id = id;
            Name = name;
            MainDefence = mainDefence;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(MainDefence)}: {MainDefence}" +
                   $", {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }
    }
}