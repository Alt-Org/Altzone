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
    public enum GestaltCycle
    {
        None = 0,
        Desensitisation = 1,
        Deflection = 2,
        Introjection = 3,
        Projection = 4,
        Retroflection = 5,
        Egotism = 6,
        Confluence = 7,
    }
}