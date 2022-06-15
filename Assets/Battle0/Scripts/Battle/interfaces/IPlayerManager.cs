using System;

namespace Battle0.Scripts.Battle.interfaces
{
    internal interface IPlayerManager
    {
        void StartCountdown(Action countdownFinished);
        void StartGameplay();
        void StopGameplay();
    }
}