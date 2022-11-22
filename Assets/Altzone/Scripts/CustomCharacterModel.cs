using UnityEngine.Assertions;

namespace Altzone.Scripts
{
    /// <summary>
    /// Model for character customization.
    /// </summary>
    public class CustomCharacterModel
    {
        public readonly int Id;
        public readonly int CharacterModelId;
        public string Name { get; set; }
        public int Speed { get; set; }
        public int Resistance { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }

        public CustomCharacterModel(int id, int characterModelId, string name, int speed, int resistance, int attack, int defence)
        {
            Assert.IsTrue(id > 0, "id > 0");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name), "string.IsNullOrWhiteSpace(name)");
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