using Altzone.Scripts.Model;

namespace Altzone.Scripts.Temp
{
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