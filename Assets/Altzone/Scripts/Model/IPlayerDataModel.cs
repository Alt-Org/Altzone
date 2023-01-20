namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Player attributes etc. for the gameplay and/or meta game.
    /// </summary>
    public interface IPlayerDataModel
    {
        int Id { get; }
        int BackpackCapacity { get; }
    }
}