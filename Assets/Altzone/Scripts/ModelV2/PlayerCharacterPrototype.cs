using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2.Internal;
using UnityEngine;

namespace Altzone.Scripts.ModelV2
{
    /// <summary>
    /// General contract for read-only player character prototype.
    /// </summary>
    public class PlayerCharacterPrototype
    {
        public class SpecialAttribute
        {
            public readonly int Value;
            public readonly int Level;
            public readonly ValueStrength Coefficient;

            public SpecialAttribute(int value, int level, ValueStrength coefficient)
            {
                Value = value;
                Level = level;
                Coefficient = coefficient;
            }
        }

        public string Id => _characterSpec.Id;
        public CharacterID CharacterId => _characterSpec.CharacterId;
        public CharacterClassID ClassType => _characterSpec.ClassType;
        public string Name => _characterSpec.Name;
        public Sprite GalleryImage => _characterSpec.GalleryImage;

        public readonly SpecialAttribute Hp;
        public readonly SpecialAttribute Speed;
        public readonly SpecialAttribute Resistance;
        public readonly SpecialAttribute Attack;
        public readonly SpecialAttribute Defence;

        private readonly CharacterSpec _characterSpec;

        internal PlayerCharacterPrototype(CharacterSpec characterSpec)
        {
            _characterSpec = characterSpec;
            Hp = new SpecialAttribute((int)BaseCharacter.GetStatValue(StatType.Hp, _characterSpec.Hp.Level),
                _characterSpec.Hp.Level, _characterSpec.Hp.Coefficient);
            Speed = new SpecialAttribute((int)BaseCharacter.GetStatValue(StatType.Speed, _characterSpec.Speed.Level),
                _characterSpec.Speed.Level, _characterSpec.Speed.Coefficient);
            Resistance =
                new SpecialAttribute(
                    (int)BaseCharacter.GetStatValue(StatType.Resistance, _characterSpec.Resistance.Level),
                    _characterSpec.Resistance.Level, _characterSpec.Resistance.Coefficient);
            Attack = new SpecialAttribute(
                (int)BaseCharacter.GetStatValue(StatType.Attack, _characterSpec.Attack.Level),
                _characterSpec.Attack.Level, _characterSpec.Attack.Coefficient);
            Defence = new SpecialAttribute(
                (int)BaseCharacter.GetStatValue(StatType.Defence, _characterSpec.Defence.Level),
                _characterSpec.Defence.Level, _characterSpec.Defence.Coefficient);
        }

        public override string ToString() => $"{_characterSpec}";
    }
}
