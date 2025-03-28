//#define DEBUG_LOG_STATE

using Photon.Deterministic;
using Quantum.QuantumUser.Simulation.SoulWall;
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
        public override void OnInit(Frame f)
        {
            Log.Debug("[GameControlSystem] OnInit");

            BattleArenaSpec battleArenaSpec = f.FindAsset(f.RuntimeConfig.BattleArenaSpec);

            GridManager.Init(battleArenaSpec);
            PlayerManager.Init(f);

            f.Events.GridSet();

            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();
            gameSession->GameInitialized = true;
        }

        public override void Update(Frame f)
        {
            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();

#if DEBUG_LOG_STATE
            Debug.Log($"[GameControlSystem] Current State: {gameSession->state}, TimeUntilStart: {gameSession->TimeUntilStart}");
#endif

            switch (gameSession->state)
            {
                case GameState.InitializeGame:
                    if (gameSession->GameInitialized) gameSession->state = GameState.CreateMap;
                    break;

                case GameState.CreateMap:
                    CreateMap(f);
                    gameSession->state = GameState.Countdown;
                    break;

                // Countdown state handling
                case GameState.Countdown:
                    gameSession->TimeUntilStart -= f.DeltaTime;

                    // Transition from Countdown to GetReadyToPlay
                    if (gameSession->TimeUntilStart < 1)
                    {
                        gameSession->state = GameState.GetReadyToPlay;
                        gameSession->TimeUntilStart = 1; // Set 1 second for the GetReadyToPlay state
                    }
                    break;

                // GetReadyToPlay state handling
                case GameState.GetReadyToPlay:
                    gameSession->TimeUntilStart -= f.DeltaTime;

                    // Transition from GetReadyToPlay to Playing
                    if (gameSession->TimeUntilStart <= 0)
                    {
                        gameSession->state = GameState.Playing;
                    }
                    break;
            }
        }

        private static void CreateMap(Frame f)
        {
            BattleArenaSpec battleArenaSpec = f.FindAsset(f.RuntimeConfig.BattleArenaSpec);
            SoulWallSpec soulWallSpec = f.FindAsset(f.RuntimeConfig.SoulWallSpec);

            SoulWallSystem.CreateSoulWalls(f, battleArenaSpec, soulWallSpec);

            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();
        }
    }
}
