namespace Altzone.Scripts.Temp
{
    /// <summary>
    /// Data Transfer Object for <c>IClanModel</c>.
    /// </summary>
    /// <remarks>
    /// Object Id is assigned by remote server.
    /// </remarks>
    public class ClanModel : AbstractModel
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int GameCoins { get; set; }

        public ClanModel(string name, string tag, int gameCoins) : base(0)
        {
            Name = name;
            Tag = tag;
            GameCoins = gameCoins;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Name)}: {Name}, {nameof(Tag)}: {Tag}, {nameof(GameCoins)}: {GameCoins}";
        }
    }
}