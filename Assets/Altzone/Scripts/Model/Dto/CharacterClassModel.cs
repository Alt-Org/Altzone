using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>ICharacterClassModel</c>.
    /// </summary>
    public class CharacterClassModel : AbstractModel, ICharacterClassModel
    {
        public string Name { get; }
        public Defence MainDefence { get; }
        public int Speed { get; }
        public int Resistance { get; }
        public int Attack { get; }
        public int Defence { get; }

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