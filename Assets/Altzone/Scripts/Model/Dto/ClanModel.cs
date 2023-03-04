using GameServer.Scripts.Dto;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>IClanModel</c>.
    /// </summary>
    public class ClanModel : AbstractModel, IClanModel
    {
        public string Name { get; }
        public string Tag { get; }
        public int GameCoins { get; }

        public ClanModel(int id, string name, string tag, int gameCoins) : base(id)
        {
            Name = name;
            Tag = tag;
            GameCoins = gameCoins;
        }

        public ClanModel(ClanDto dto) : base(dto.Id)
        {
            Name = dto.Name;
            Tag = dto.Tag;
            GameCoins = dto.GameCoins;
        }
    }
}