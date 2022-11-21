namespace Altzone.Scripts
{
    /// <summary>
    /// Model for character customization.
    /// </summary>
    public class CustomCharacterModel
    {
        public readonly int Id;
        public readonly int CharacterModelId;
        public readonly string Name;
        public readonly int Speed;
        public readonly int Resistance;
        public readonly int Attack;
        public readonly int Defence;

        public CustomCharacterModel(int id, int characterModelId, string name, int speed, int resistance, int attack, int defence)
        {
            Id = id;
            CharacterModelId = characterModelId;
            Name = name;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }
    }
}