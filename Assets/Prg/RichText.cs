/// <summary>
/// UNITY Rich Text formatting wrapper.
/// </summary>
public static class RichText
{
    public static string Bold(string text)
    {
#if UNITY_EDITOR
        return $"<b>{text}</b>";
#else
        return text;
#endif
    }

    public static string White(string text)
    {
#if UNITY_EDITOR
        return $"<color=white>{text}</color>";
#else
        return text;
#endif
    }

    public static string Red(string text)
    {
#if UNITY_EDITOR
        return $"<color=red>{text}</color>";
#else
        return text;
#endif
    }

    public static string Blue(string text)
    {
#if UNITY_EDITOR
        return $"<color=blue>{text}</color>";
#else
        return text;
#endif
    }

    public static string Magenta(string text)
    {
#if UNITY_EDITOR
            return $"<color=magenta>{text}</color>";
#else
        return text;
#endif
    }

    public static string Yellow(string text)
    {
#if UNITY_EDITOR
#else
        return text;
#endif
        return $"<color=yellow>{text}</color>";
    }

    public static string Brown(string text)
    {
#if UNITY_EDITOR
        return $"<color=brown>{text}</color>";
#else
        return text;
#endif
    }
}