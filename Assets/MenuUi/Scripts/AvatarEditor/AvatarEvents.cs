using System;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;

public static class AvatarEvents
{
    public static event Action<PlayerData> AvatarSaved;
    public static PlayerData UpdatedPlayerData;
    public static void RaiseAvatarSaved(PlayerData data)
    {
        AvatarSaved?.Invoke(data);
        UpdatedPlayerData = data;
    }
}
