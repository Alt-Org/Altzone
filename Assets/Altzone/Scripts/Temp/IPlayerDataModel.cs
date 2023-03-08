namespace Altzone.Scripts.Temp
{
    /// <summary>
    /// Player attributes etc. for the gameplay and/or meta game.
    /// </summary>
    public interface IPlayerDataModel
    {
        int Id { get; }
        int ClanId { get; }
        int CurrentCharacterModelId { get; set; }
        string Name { get; set; }
        int BackpackCapacity { get; }
    }
}