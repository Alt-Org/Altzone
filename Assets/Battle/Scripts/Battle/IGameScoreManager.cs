using Photon.Realtime;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IGameScoreManager
    {
        Vector2 BlueScore { get; }
        Vector2 RedScore { get; }
        
        void ResetScores();
        
        void OnHeadCollision(Collision2D collision);

        void OnWallCollision(Collision2D collision);

        void ShowGameOverWindow(Room room, int winType, int winningTeam, int blueScore, int redScore);
    }
}