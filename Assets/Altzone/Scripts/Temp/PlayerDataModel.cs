namespace Altzone.Scripts.Temp
{
    /// <summary>
    /// Data Transfer Object for <c>IPlayerDataModel</c>.<br />
    /// Note that <c>PlayerGuid</c> is used to identify this object on remote server.
    /// </summary>
    /// <remarks>
    /// Object Id is assigned by remote server.
    /// </remarks>
    public class PlayerDataModel : AbstractModel, IPlayerDataModel
    {
        public string PlayerGuid { get; }
        public int ClanId { get; set; }
        public int CurrentCharacterModelId { get; set; }
        public string Name { get; set; }
        public int BackpackCapacity { get; set; }

        public PlayerDataModel(string playerGuid, int clanId, int currentCharacterModelId, string name, int backpackCapacity) : base(1)
        {
            PlayerGuid = playerGuid;
            ClanId = clanId;
            CurrentCharacterModelId = currentCharacterModelId;
            Name = name;
            BackpackCapacity = backpackCapacity;
        }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(ClanId)}: {ClanId}, {nameof(CurrentCharacterModelId)}: {CurrentCharacterModelId}" +
                $", {nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(PlayerGuid)}: {PlayerGuid}";
        }
    }
}