using Photon.Realtime;

namespace Battle.Scripts.Battle
{
    internal interface IGameScoreManager
    {
        void ShowGameOverWindow(Room room, int winType, int winningTeam, int blueScore, int redScore);
    }
}