using Altzone.Scripts.Model;

namespace Altzone.Scripts.Battle
{
    /// <summary>
    /// Read-only interface of Player character for Battle gameplay.
    /// </summary>
    public interface IBattleCharacter
    {
        Defence MainDefence { get; }
        int Speed { get; }
        int Resistance { get; }
        int Attack { get; }
        int Defence { get; }        
    }
}