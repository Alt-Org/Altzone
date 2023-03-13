/// <summary>
/// UNITY Rich Text formatting wrapper.
/// </summary>
public static class RichText
{
    public static string Bold(object text)
    {
#if UNITY_EDITOR
        return $"<b>{text}</b>";
#else
        return text?.ToString();
#endif
    }

    public static string White(object text)
    {
#if UNITY_EDITOR
        return $"<color=white>{text}</color>";
#else
        return text?.ToString();
#endif
    }

    public static string Red(object text)
    {
#if UNITY_EDITOR
        return $"<color=red>{text}</color>";
#else
        return text?.ToString();
#endif
    }

    public static string Blue(object text)
    {
#if UNITY_EDITOR
        return $"<color=blue>{text}</color>";
#else
        return text?.ToString();
#endif
    }

    public static string Green(object text)
    {
#if UNITY_EDITOR
        return $"<color=green>{text}</color>";
#else
        return text?.ToString();
#endif
    }

    public static string Magenta(object text)
    {
#if UNITY_EDITOR
        return $"<color=magenta>{text}</color>";
#else
        return text?.ToString();
#endif
    }

    public static string Yellow(object text)
    {
#if UNITY_EDITOR
        return $"<color=yellow>{text}</color>";
#else
        return text?.ToString();
#endif
    }

    public static string Brown(object text)
    {
#if UNITY_EDITOR
        return $"<color=brown>{text}</color>";
#else
        return text?.ToString();
#endif
    }
}