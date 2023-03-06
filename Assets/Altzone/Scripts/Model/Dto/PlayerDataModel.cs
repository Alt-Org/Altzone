using System;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>IPlayerDataModel</c>.
    /// </summary>
    [Serializable]
    public class PlayerDataModel : AbstractModel, IPlayerDataModel
    {
        public int ClanId { get; set; }
        public int BackpackCapacity { get; set; }

        public PlayerDataModel(int id, int clanId, int backpackCapacity) : base(id)
        {
            ClanId = clanId;
            BackpackCapacity = backpackCapacity;
        }
    }
}