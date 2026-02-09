using System;
using Altzone.Scripts.Model.Poco.Player;

public static class AvatarEvents
{
    public static event Action<PlayerData> AvatarSaved;

    public static void RaiseAvatarSaved(PlayerData data)
    {
        AvatarSaved?.Invoke(data);
    }
}
