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
        public readonly string CharacterClassName;
        public readonly int CustomCharacterModelId;
        public readonly int CharacterClassModelId;
        public readonly int PlayerPrefabId;
        public readonly Defence MainDefence;
        public readonly int Speed;
        public readonly int Resistance;
        public readonly int Attack;
        public readonly int Defence;

        internal BattleCharacter(string name, string characterClassName, int customCharacterModelId, int characterClassModelId, int playerPrefabId,
            Defence mainDefence, int speed, int resistance, int attack, int defence)
        {
            Name = name;
            CharacterClassName = characterClassName;
            CustomCharacterModelId = customCharacterModelId;
            CharacterClassModelId = characterClassModelId;
            PlayerPrefabId = playerPrefabId;
            MainDefence = mainDefence;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }
    }
}