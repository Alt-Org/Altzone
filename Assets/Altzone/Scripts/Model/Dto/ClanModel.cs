using GameServer.Scripts.Dto;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>IClanModel</c>.
    /// </summary>
    public class ClanModel : AbstractModel, IClanModel
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int GameCoins { get; set; }

        public ClanModel(int id, string name, string tag, int gameCoins) : base(id)
        {
            Name = name;
            Tag = tag;
            GameCoins = gameCoins;
        }

        internal ClanModel(ClanDto dto) : base(dto.Id)
        {
            Name = dto.Name;
            Tag = dto.Tag;
            GameCoins = dto.GameCoins;
        }

        internal ClanDto ToDto()
        {
            return new ClanDto
            {
                Id = Id,
                GameCoins = GameCoins,
                Name = Name,
                Tag = Tag
            };
        }
    }
}