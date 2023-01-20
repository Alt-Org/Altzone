using System;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>IPlayerDataModel</c>.
    /// </summary>
    [Serializable]
    public class PlayerDataModel : IPlayerDataModel
    {
        public int _id;
        public int _backpackCapacity;

        public int Id => _id;

        public int BackpackCapacity => _backpackCapacity;

        public PlayerDataModel(int id, int backpackCapacity)
        {
            _id = id;
            _backpackCapacity = backpackCapacity;
        }
    }
}