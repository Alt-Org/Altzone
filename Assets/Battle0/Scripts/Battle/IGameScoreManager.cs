using System;
using Photon.Realtime;
using UnityEngine;

namespace Battle0.Scripts.Battle
{
    internal interface IGameScoreManager
    {
        Tuple<int,int> BlueScore { get; }
        Tuple<int,int> RedScore { get; }
        
        void ResetScores();
        
        void OnHeadCollision(Collision2D collision);

        void OnWallCollision(Collision2D collision);

        void ShowGameOverWindow(Room room, int winType, int winningTeam, int blueScore, int redScore);
    }
}