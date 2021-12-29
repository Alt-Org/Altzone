/// <summary>
/// UNITY Rich Text formatting wrapper.
/// </summary>
public static class RichText
{
    public static string White(string text)
    {
        return $"<color=white>{text}</color>";
    }

    public static string Red(string text)
    {
        return $"<color=red>{text}</color>";
    }

    public static string Blue(string text)
    {
        return $"<color=blue>{text}</color>";
    }

    public static string Magenta(string text)
    {
        return $"<color=magenta>{text}</color>";
    }

    public static string Yellow(string text)
    {
        return $"<color=yellow>{text}</color>";
    }

    public static string Brown(string text)
    {
        return $"<color=brown>{text}</color>";
    }
}