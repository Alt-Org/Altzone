/// @file BattleGameControlQSystem.cs
/// <summary>
/// Controls the overall game state flow in Quantum simulation.
/// </summary>
///
/// This system initializes the battle grid and player manager, and controls game state transitions from initialization to active gameplay.

//#define DEBUG_LOG_STATE

using UnityEngine.Scripting;
using Quantum;

using Battle.QSimulation.Player;
using Battle.QSimulation.SoulWall;
using Photon.Deterministic;

namespace Battle.QSimulation.Game
{
    /**
     *  Systems that monitor game state:
     *  -ProjectileSpawnerSystem
     */
    [Preserve]
    public unsafe class BattleGameControlQSystem : SystemMainThread, ISignalOnPlayerAdded
    {
        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System OnInit method</a> gets called when the system is initialized.</span><br/>
        /// Initializes the arena, player system, and sets the game session as initialized.
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public override void OnInit(Frame f)
        {
            Log.Debug("[GameControlSystem] OnInit");

            BattleArenaQSpec battleArenaSpec = BattleQConfig.GetArenaSpec(f);

            BattleGridManager.Init(battleArenaSpec);
            BattlePlayerManager.Init(f, battleArenaSpec);

            BattleGameSessionQSingleton* gameSession = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();
            gameSession->GameTimeSec = 0;
            gameSession->GameInitialized = true;
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Signal method@u-exlink</a>
        /// that gets called when <a href="https://doc-api.photonengine.com/en/quantum/current/interface_quantum_1_1_i_signal_on_player_added.html">ISignalOnPlayerAdded</a> is sent.</span><br/>
        /// Called when a player is added for the first time. Registers the player in BattlePlayerManager.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerRef">Reference to the player.</param>
        /// <param name="firstTime">True if this is the first join.</param>
        public void OnPlayerAdded(Frame f, PlayerRef playerRef, bool firstTime)
        {
            BattlePlayerManager.RegisterPlayer(f, playerRef);
        }

        public static void OnGameOver(Frame f, BattleTeamNumber winningTeam)
        {
            BattleGameSessionQSingleton* gameSession = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();
            f.Events.BattleViewGameOver(winningTeam, gameSession->GameTimeSec);
            gameSession->State = BattleGameState.GameOver;

            BattleTeamNumber WinningTeam = winningTeam;
            f.Signals.BattleOnGameOver(WinningTeam);
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method</a> gets called every frame.</span><br/>
        /// Controls state transitions of the game session per frame. Manages countdowns and progression to 'Playing'.
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public override void Update(Frame f)
        {
            BattleGameSessionQSingleton* gameSession = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();

#if DEBUG_LOG_STATE
            Debug.Log($"[GameControlSystem] Current State: {gameSession->state}, TimeUntilStart: {gameSession->TimeUntilStart}");
#endif

            switch (gameSession->State)
            {
                case BattleGameState.InitializeGame:
                    if (gameSession->GameInitialized)
                    {
                        BattleWaitForPlayersData data = new();
                        string[] playerNames = BattleParameters.GetPlayerNames(f);
                        for (int i = 0; i < playerNames.Length; i++)
                        {
                            data.PlayerNames[i] = playerNames[i];
                        }

                        f.Events.BattleViewWaitForPlayers(data);
                        gameSession->State = BattleGameState.WaitForPlayers;
                    }
                    break;

                case BattleGameState.WaitForPlayers:
                    if (BattlePlayerManager.IsAllPlayersRegistered(f))
                    {
                        f.Events.BattleViewAllPlayersConnected();
                        f.Events.BattleViewInit();
                        gameSession->State = BattleGameState.CreateMap;
                    }
                    break;

                case BattleGameState.CreateMap:
                    CreateMap(f);
                    gameSession->State = BattleGameState.WaitToStart;
                    break;

                case BattleGameState.WaitToStart:
                    gameSession->LoadDelaySec -= f.DeltaTime;

                    if (gameSession->LoadDelaySec <= FP._0)
                    {
                        f.Events.BattleViewActivate();
                        gameSession->State = BattleGameState.Countdown;
                    }
                    break;

                // Countdown state handling
                case BattleGameState.Countdown:
                    gameSession->TimeUntilStartSec -= f.DeltaTime;

                    // Transition from Countdown to GetReadyToPlay
                    if (gameSession->TimeUntilStartSec < FP._1)
                    {
                        f.Events.BattleViewGetReadyToPlay();
                        gameSession->State = BattleGameState.GetReadyToPlay;
                        gameSession->TimeUntilStartSec = FP._1; // Set 1 second for the GetReadyToPlay state
                    }
                    break;

                // GetReadyToPlay state handling
                case BattleGameState.GetReadyToPlay:
                    gameSession->TimeUntilStartSec -= f.DeltaTime;

                    // Transition from GetReadyToPlay to Playing
                    if (gameSession->TimeUntilStartSec <= FP._0)
                    {
                        f.Events.BattleViewGameStart();
                        gameSession->State = BattleGameState.Playing;
                    }
                    break;

                case BattleGameState.Playing:
                    gameSession->GameTimeSec += f.DeltaTime;
                    break;
            }
        }

        /// <summary>
        /// Sets up the game map during the 'CreateMap' game state.
        /// That includes SoulWalls and players. Other parts of the map don't need setup.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        private static void CreateMap(Frame f)
        {
            BattleArenaQSpec battleArenaSpec = BattleQConfig.GetArenaSpec(f);
            BattleSoulWallQSpec soulWallSpec = BattleQConfig.GetSoulWallSpec(f);

            BattleSoulWallQSystem.CreateSoulWalls(f, battleArenaSpec, soulWallSpec);

            BattlePlayerManager.CreatePlayers(f);
            BattlePlayerQSystem.SpawnPlayers(f);
        }
    }
}
