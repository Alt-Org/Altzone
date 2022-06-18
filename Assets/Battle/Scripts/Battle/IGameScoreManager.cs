using Photon.Realtime;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IGameScoreManager
    {
        void Reset();
        
        void OnHeadCollision(Collision2D collision);

        void OnWallCollision(Collision2D collision);

        void ShowGameOverWindow(Room room, int winType, int winningTeam, int blueScore, int redScore);
    }
}