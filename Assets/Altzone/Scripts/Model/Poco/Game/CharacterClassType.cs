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
    public enum CharacterClassType
    {
        None = 0,
        Desensitizer = 100,
        Trickster = 200,
        Obedient = 300,
        Projector = 400,
        Retroflector = 500,
        Confluent = 600,
        Intellectualizer = 700
    }
}
