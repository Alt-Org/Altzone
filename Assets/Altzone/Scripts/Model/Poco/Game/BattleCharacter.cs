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
        public readonly int CustomCharacterId;
        public readonly string Name;
        public readonly int CharacterClassId;
        public readonly string CharacterClassName;
        public readonly string UnityKey;
        public readonly Defence MainDefence;
        public readonly int Speed;
        public readonly int Resistance;
        public readonly int Attack;
        public readonly int Defence;

        private BattleCharacter(int customCharacterId, string name, int characterClassId, string characterClassName, string unityKey,
            Defence mainDefence, int speed, int resistance, int attack, int defence)
        {
            CustomCharacterId = customCharacterId;
            Name = name;
            CharacterClassId = characterClassId;
            CharacterClassName = characterClassName;
            UnityKey = unityKey;
            MainDefence = mainDefence;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        internal static BattleCharacter Create(CustomCharacter customCharacter, CharacterClass characterClass)
        {
            Assert.AreEqual(customCharacter.CharacterClassId, characterClass.CharacterClassId, "CharacterClassId mismatch");
            return new BattleCharacter(
                customCharacter.CharacterClassId, customCharacter.Name,
                characterClass.CharacterClassId, characterClass.Name,
                customCharacter.UnityKey,
                (Defence)characterClass.Defence,
                customCharacter.Speed + characterClass.Speed,
                customCharacter.Resistance + characterClass.Resistance,
                customCharacter.Attack + characterClass.Attack,
                customCharacter.Defence + characterClass.Defence);
        }

        public override string ToString()
        {
            return $"CustomCharacter: {CustomCharacterId} : {Name}" +
                   $", CharacterClass: {CharacterClassId} : {CharacterClassName}" +
                   $", UnityKey: {UnityKey}, MainDefence: {MainDefence}" +
                   $", Speed: {Speed}, Resistance: {Resistance}, Attack: {Attack}, Defence: {Defence}";
        }
    }
}