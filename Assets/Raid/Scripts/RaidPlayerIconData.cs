using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;

public readonly struct RaidPlayerIconData
{
    public RaidPlayerIconData(string playerName, CharacterID characterId, AvatarData avatarData)
    {
        PlayerName = playerName;
        CharacterId = characterId;
        AvatarData = avatarData;
    }

    public string PlayerName { get; }
    public CharacterID CharacterId { get; }
    public AvatarData AvatarData { get; }
}
