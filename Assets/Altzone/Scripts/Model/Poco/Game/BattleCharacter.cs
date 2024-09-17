using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Non-mutable gameplay character for Battle.
    /// </summary>
    /// <remarks>
    /// This is a combination of <c>CustomCharacter</c> and its related <c>CharacterClass</c>
    /// </remarks>
    public class BattleCharacter
    {
        public readonly CharacterID CharacterID;
        public readonly string Name;
        public readonly int Hp;
        public readonly int Speed;
        public readonly int Resistance;
        public readonly int Attack;
        public readonly int Defence;

        public CharacterClassID CharacterClassID => CustomCharacter.GetClassID(CharacterID);

        public BattleCharacter(CharacterID customCharacterId, string name,
            int hp, int speed, int resistance, int attack, int defence)
        {
            //Assert.AreNotEqual(CharacterID.None, customCharacterId);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(name));
            CharacterID = customCharacterId;
            Name = name;
            Hp = hp;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        internal static BattleCharacter Create(CustomCharacter customCharacter, CharacterClass characterClass)
        {
            //Assert.AreEqual(customCharacter.CharacterClassId, characterClass.Id, "CharacterClassId mismatch");
            return new BattleCharacter(
                customCharacter.Id,
                customCharacter.Name,
                customCharacter.Hp + characterClass.Hp,
                customCharacter.Speed + characterClass.Speed,
                customCharacter.Resistance + characterClass.Resistance,
                customCharacter.Attack + characterClass.Attack,
                customCharacter.Defence + characterClass.Defence);
        }

        internal static BattleCharacter CreateEmpty()
        {
            //Assert.AreEqual(customCharacter.CharacterClassId, characterClass.Id, "CharacterClassId mismatch");
            return new BattleCharacter(
                CharacterID.None,
                "Dummy",
                0,
                0,
                0,
                0,
                0);
        }

        public override string ToString()
        {
            return $"CustomCharacter: {CharacterID} : {Name}" +
                   $", Hp: {Hp}, Speed: {Speed}, Resistance: {Resistance}, Attack: {Attack}, Defence: {Defence}";
        }
    }
}
