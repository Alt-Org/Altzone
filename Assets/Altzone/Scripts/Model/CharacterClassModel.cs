using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Enum values for Defence "attribute".
    /// </summary>
    /// <remarks>
    /// This can be serialized so do not change or remove enum values.
    /// </remarks>
    public enum Defence
    {
        None = 0,
        Desensitisation = 1,
        Deflection = 2,
        Introjection = 3,
        Projection = 4,
        Retroflection = 5,
        Egotism = 6,
        Confluence = 7,
    }

    /// <summary>
    /// Game base character class model.
    /// </summary>
    public class CharacterClassModel : AbstractModel
    {
        public readonly string Name;
        public readonly Defence MainDefence;
        public readonly int Speed;
        public readonly int Resistance;
        public readonly int Attack;
        public readonly int Defence;

        public CharacterClassModel(int id, string name, Defence mainDefence, int speed, int resistance, int attack, int defence) : base(id)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(name), "string.IsNullOrWhiteSpace(name)");
            Name = name;
            MainDefence = mainDefence;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Name)}: {Name}, {nameof(MainDefence)}: {MainDefence}, {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }
    }
}