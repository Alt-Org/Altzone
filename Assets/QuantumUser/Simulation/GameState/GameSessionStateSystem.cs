using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    /**
     *  Systems that monitor game state:
     *  -ProjectileSpawnerSystem
     */
    [Preserve]
    public unsafe class GameSessionStateSystem : SystemMainThreadFilter<GameSessionStateSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public GameSession* GameSession;
        }

        public override void OnInit(Frame f)
        {
            Log.Debug("Quantum GameSessionStateSystem OnInit");
        }

        public override void Update(Frame f, ref Filter filter)
        {
            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();
            if (gameSession == null)
                return;

            // Debug the current state
            //Debug.Log($"Current State: {gameSession->state}, TimeUntilStart: {gameSession->TimeUntilStart}");

            //Countdown state handling
            if (gameSession->CountDownStarted)
            {
                gameSession->TimeUntilStart -= f.DeltaTime;
            }

            // Transition from Countdown to GetReadyToPlay
            if (gameSession->TimeUntilStart < 1 && gameSession->state == GameState.Countdown)
            {
                gameSession->state = GameState.GetReadyToPlay;
                gameSession->TimeUntilStart = 1; // Set 1 second for the GetReadyToPlay state
            }

            // Transition from GetReadyToPlay to Playing
            if (gameSession->state == GameState.GetReadyToPlay)
            {
                gameSession->TimeUntilStart -= f.DeltaTime;

                if (gameSession->TimeUntilStart <= 0)
                {
                    gameSession->state = GameState.Playing;
                }
            }
        }
    }
}
