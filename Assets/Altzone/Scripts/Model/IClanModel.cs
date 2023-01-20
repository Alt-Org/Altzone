namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Clan base data.
    /// </summary>
    public interface IClanModel
    {
        string Name { get; }
        string Tag { get; }
        int GameCoins { get; }
        int Id { get; }
    }
}