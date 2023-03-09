namespace Altzone.Scripts.Model.Poco
{
    /// <summary>
    /// Gameplay character for Battle.
    /// </summary>
    /// <remarks>
    /// This is a combination of <c>CustomCharacter</c> and its related <c>CharacterClass</c>
    /// </remarks>
    public class BattleCharacter
    {
        public readonly string Name;
        public readonly int CustomCharacterId;
        public readonly string CharacterClassName;
        public readonly int CharacterClassId;
        public readonly string PlayerPrefabKey;
        public readonly Defence MainDefence;
        public readonly int Speed;
        public readonly int Resistance;
        public readonly int Attack;
        public readonly int Defence;

        private BattleCharacter(string name, int customCharacterId, string characterClassName, int characterClassId, string playerPrefabKey,
            Defence mainDefence, int speed, int resistance, int attack, int defence)
        {
            Name = name;
            CharacterClassName = characterClassName;
            CustomCharacterId = customCharacterId;
            CharacterClassId = characterClassId;
            PlayerPrefabKey = playerPrefabKey;
            MainDefence = mainDefence;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        internal static BattleCharacter Create(CustomCharacter customCharacter, CharacterClass characterClass)
        {
            return new BattleCharacter(
                customCharacter.Name, customCharacter.CharacterClassId,
                characterClass.Name, characterClass.Id,
                customCharacter.PlayerPrefabKey,
                (Defence)characterClass.Defence,
                customCharacter.Speed + characterClass.Speed,
                customCharacter.Resistance + characterClass.Resistance,
                customCharacter.Attack + characterClass.Attack,
                customCharacter.Defence + characterClass.Defence);
        }
    }
}