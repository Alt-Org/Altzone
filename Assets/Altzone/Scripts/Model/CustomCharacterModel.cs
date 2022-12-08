using System;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Public interface for <c>CustomCharacterModel</c>.
    /// </summary>
    /// <remarks>
    /// See https://github.com/Alt-Org/Altzone/wiki/Battle-Pelihahmo
    /// </remarks>
    public interface ICustomCharacterModel
    {
        public int Id { get; }
        public int CharacterModelId { get; }
        public string Name { get; set; }
        public int Speed { get; set; }
        public int Resistance { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
    }

    /// <summary>
    /// Serializable Model for character customization.
    /// </summary>
    /// <remarks>
    /// JsonUtility can only serialize public fields :-(
    /// </remarks>
    [Serializable]
    public class CustomCharacterModel : ICustomCharacterModel
    {
        public int _id;
        public int _characterModelId;
        public string _name;
        public int _speed;
        public int _resistance;
        public int _attack;
        public int _defence;

        int ICustomCharacterModel.Id => _id;

        int ICustomCharacterModel.CharacterModelId => _characterModelId;

        string ICustomCharacterModel.Name
        {
            get => _name;
            set => _name = value;
        }

        int ICustomCharacterModel.Speed
        {
            get => _speed;
            set => _speed = value;
        }

        int ICustomCharacterModel.Resistance
        {
            get => _resistance;
            set => _resistance = value;
        }

        int ICustomCharacterModel.Attack
        {
            get => _attack;
            set => _attack = value;
        }

        int ICustomCharacterModel.Defence
        {
            get => _defence;
            set => _defence = value;
        }

        public CustomCharacterModel(int id, int characterModelId, string name, int speed, int resistance, int attack, int defence)
        {
            Assert.IsTrue(id > 0, "id > 0");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name), "string.IsNullOrWhiteSpace(name)");
            _id = id;
            _characterModelId = characterModelId;
            _name = name;
            _speed = speed;
            _resistance = resistance;
            _attack = attack;
            _defence = defence;
        }
    }
}