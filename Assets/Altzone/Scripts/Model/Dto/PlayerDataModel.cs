using GameServer.Scripts.Dto;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>IPlayerDataModel</c>.
    /// </summary>
    /// <remarks>
    /// Object Id is assigned by remote server.
    /// </remarks>
    public class PlayerDataModel : AbstractModel, IPlayerDataModel
    {
        public int ClanId { get; set; }
        public int CurrentCharacterModelId { get; set; }
        public string Name { get; set; }
        public int BackpackCapacity { get; set; }

        public PlayerDataModel(int clanId, int currentCharacterModelId, string name, int backpackCapacity) : base(0)
        {
            ClanId = clanId;
            CurrentCharacterModelId = currentCharacterModelId;
            Name = name;
            BackpackCapacity = backpackCapacity;
        }

        internal PlayerDataModel(PlayerDto dto) : base(dto.Id)
        {
            ClanId = dto.ClanId;
            CurrentCharacterModelId = dto.CurrentCharacterModelId;
            Name = dto.Name;
            BackpackCapacity = dto.BackpackCapacity;
        }

        internal PlayerDto ToDto()
        {
            return new PlayerDto
            {
                Id = Id,
                ClanId = ClanId,
                CurrentCharacterModelId = CurrentCharacterModelId,
                Name = Name,
                BackpackCapacity = BackpackCapacity
            };
        }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(ClanId)}: {ClanId}, {nameof(CurrentCharacterModelId)}: {CurrentCharacterModelId}, {nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}";
        }
    }
}