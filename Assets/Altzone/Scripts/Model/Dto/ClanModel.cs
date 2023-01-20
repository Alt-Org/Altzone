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
    }
}