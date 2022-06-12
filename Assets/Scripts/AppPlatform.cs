using UnityEngine;

public static class AppPlatform
{
    public static bool IsEditor { get; } = Application.platform.ToString().ToLower().EndsWith("editor");

    public static bool IsDevelopmentBuild
    {
        get
        {
            if (IsEditor)
            {
                return true;
            }
#if DEVELOPMENT_BUILD
                return true;
#endif
            return false;
        }
    }
}