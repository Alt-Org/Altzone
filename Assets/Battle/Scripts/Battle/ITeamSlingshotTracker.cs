using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface ITeamSlingshotTracker
    {
        float GetSqrDistance { get; }

        Transform Player1Transform { get; }
        Transform Player2Transform { get; }

        public void StopTracking();
    }
}