//#define DEBUG_LOG_STATE

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
    public unsafe class GameControlSystem : SystemMainThread
    {
        /// <summary>
        /// "Constant" BattleArenaSpec reference. (DO NOT SET AFTER INIT)
        /// </summary>
        public static BattleArenaSpec BattleArenaSpec { get; private set; }

        public override void OnInit(Frame f)
        {
            Log.Debug("[GameControlSystem] OnInit");
            BattleArenaSpec = f.FindAsset<BattleArenaSpec>("QuantumUser/Resources/Specs/BattleArenaSpec");

            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();
            gameSession->state = GameState.Countdown;
        }

        public override void Update(Frame f)
        {
            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();

#if DEBUG_LOG_STATE
            Debug.Log($"[GameControlSystem] Current State: {gameSession->state}, TimeUntilStart: {gameSession->TimeUntilStart}");
#endif

            // Countdown state handling
            if (gameSession->state == GameState.Countdown)
            {
                gameSession->TimeUntilStart -= f.DeltaTime;

                // Transition from Countdown to GetReadyToPlay
                if (gameSession->TimeUntilStart < 1)
                {
                    gameSession->state = GameState.GetReadyToPlay;
                    gameSession->TimeUntilStart = 1; // Set 1 second for the GetReadyToPlay state
                }
            }

            // GetReadyToPlay state handling
            if (gameSession->state == GameState.GetReadyToPlay)
            {
                gameSession->TimeUntilStart -= f.DeltaTime;

                // Transition from GetReadyToPlay to Playing
                if (gameSession->TimeUntilStart <= 0)
                {
                    gameSession->state = GameState.Playing;
                }
            }
        }
    }
}
