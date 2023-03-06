namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Player attributes etc. for the gameplay and/or meta game.
    /// </summary>
    public interface IPlayerDataModel
    {
        int Id { get; }
        int ClanId { get; }
        string Name { get; set; }
        int BackpackCapacity { get; }
    }
}