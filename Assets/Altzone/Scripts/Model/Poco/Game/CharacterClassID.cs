namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// 'Natural constant' enum values for <c>GestaltCycle</c>. See more:<br />
    /// https://www.therapyduo.com/resources/gestalt-psychotherapy/gestalt-history-theory-overview/
    /// https://www.enduringmind.co.uk/mindfulness-cycle-of-change/
    /// </summary>
    /// <remarks>
    /// This will be serialized in UNITY scenes and prefabs and sent over network. Do not change these! 
    /// </remarks>
    public enum CharacterClassID
    {
        None = 0,
        Desensitizer = 1 << 8,
        Trickster = 2 << 8,
        Obedient = 3 << 8,
        Projector = 4 << 8,
        Retroflector = 5 << 8,
        Confluent = 6 << 8,
        Intellectualizer = 7 << 8
    }
}
