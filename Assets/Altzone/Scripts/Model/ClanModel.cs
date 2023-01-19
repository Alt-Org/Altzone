namespace Altzone.Scripts.Model
{
    public class ClanModel : AbstractModel
    {
        public readonly string Name;
        public readonly string Tag;
        public readonly int GameCoins;

        public ClanModel(int id, string name, string tag, int gameCoins) : base(id)
        {
            Name = name;
            Tag = tag;
            GameCoins = gameCoins;
        }
    }
}