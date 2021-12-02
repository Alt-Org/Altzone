namespace Battle.Scripts.interfaces
{
    /// <summary>
    /// Interface to manage ball color with conflicting/challenging requirements.
    /// </summary>
    /// <remarks>
    /// Ball color depends on which side of game arena ball is traversing and whether is is ghosted or not.
    /// </remarks>
    public interface IBallColor
    {
        void initialize();
        void setNormalMode();
        void setGhostedMode();
    }
}