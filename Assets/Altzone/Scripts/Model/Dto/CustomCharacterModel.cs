using System;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>ICustomCharacterModel</c>.
    /// </summary>
    [Serializable]
    public class CustomCharacterModel : ICustomCharacterModel
    {
        public int _id;
        public int _characterModelId;
        public int _playerPrefabId;
        public string _name;
        public int _speed;
        public int _resistance;
        public int _attack;
        public int _defence;

        public void SetId(int id)
        {
            Assert.AreEqual(0, _id);
            _id = id;
        }

        int ICustomCharacterModel.Id => _id;

        int ICustomCharacterModel.CharacterModelId => _characterModelId;

        int ICustomCharacterModel.PlayerPrefabId
        {
            get => _playerPrefabId;
            set => _playerPrefabId = value;
        }

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