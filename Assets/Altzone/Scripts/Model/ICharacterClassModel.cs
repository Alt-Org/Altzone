namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Enum values for Defence "attribute".
    /// </summary>
    /// <remarks>
    /// This can be serialized so do not change or remove enum values.
    /// </remarks>
    public enum Defence
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

    /// <summary>
    /// Game base character class model.
    /// </summary>
    /// <remarks>
    /// See https://github.com/Alt-Org/Altzone/wiki/Battle-Pelihahmo
    /// </remarks>
    public interface ICharacterClassModel
    {
        string Name { get; }
        Defence MainDefence { get; }
        int Speed { get; }
        int Resistance { get; }
        int Attack { get; }
        int Defence { get; }
        int Id { get; }
    }
}