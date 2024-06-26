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
        public readonly string CustomCharacterId;
        public readonly string Name;
        public readonly string CharacterClassId;
        public readonly string CharacterClassName;
        public readonly int UnityKey;
        public readonly GestaltCycle GestaltCycle;
        public readonly int Speed;
        public readonly int Resistance;
        public readonly int Attack;
        public readonly int Defence;

        private BattleCharacter(string customCharacterId, string name, string characterClassId, string characterClassName, int unityKey,
            GestaltCycle gestaltCycle, int speed, int resistance, int attack, int defence)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(customCharacterId));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(name));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(characterClassId));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(characterClassName));
            Assert.AreNotEqual(GestaltCycle.None, gestaltCycle);
            CustomCharacterId = customCharacterId;
            Name = name;
            CharacterClassId = characterClassId;
            CharacterClassName = characterClassName;
            UnityKey = unityKey;
            GestaltCycle = gestaltCycle;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        internal static BattleCharacter Create(CustomCharacter customCharacter, CharacterClass characterClass)
        {
            Assert.AreEqual(customCharacter.CharacterClassId, characterClass.Id, "CharacterClassId mismatch");
            return new BattleCharacter(
                customCharacter.Id, customCharacter.Name,
                characterClass.Id, characterClass.Name,
                customCharacter.UnityKey,
                characterClass.GestaltCycle,
                customCharacter.Speed + characterClass.Speed,
                customCharacter.Resistance + characterClass.Resistance,
                customCharacter.Attack + characterClass.Attack,
                customCharacter.Defence + characterClass.Defence);
        }

        public override string ToString()
        {
            return $"CustomCharacter: {CustomCharacterId} : {Name}" +
                   $", CharacterClass: {CharacterClassId} : {CharacterClassName}" +
                   $", UnityKey: {UnityKey}, MainDefence: {GestaltCycle}" +
                   $", Speed: {Speed}, Resistance: {Resistance}, Attack: {Attack}, Defence: {Defence}";
        }
    }
}
