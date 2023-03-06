using System;
using GameServer.Scripts.Dto;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>IPlayerDataModel</c>.
    /// </summary>
    [Serializable]
    public class PlayerDataModel : AbstractModel, IPlayerDataModel
    {
        public int ClanId { get; set; }
        public string Name { get; set; }
        public int BackpackCapacity { get; set; }

        public PlayerDataModel(int id, int clanId, string name, int backpackCapacity) : base(id)
        {
            ClanId = clanId;
            Name = name;
            BackpackCapacity = backpackCapacity;
        }

        internal PlayerDataModel(PlayerDto dto) : base(dto.Id)
        {
            ClanId = dto.ClanId;
            Name = dto.Name;
            BackpackCapacity = dto.BackpackCapacity;
        }

        internal PlayerDto ToDto()
        {
            return new PlayerDto
            {
                Id = Id,
                ClanId = ClanId,
                Name = Name,
                BackpackCapacity = BackpackCapacity
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(ClanId)}: {ClanId}, {nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}";
        }
    }
}