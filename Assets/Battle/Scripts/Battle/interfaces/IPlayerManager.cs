using System;

namespace Battle.Scripts.Battle.interfaces
{
    internal interface IPlayerManager
    {
        void StartCountdown(Action countdownFinished);
        void StartGameplay();
        void StopGameplay();
    }
}