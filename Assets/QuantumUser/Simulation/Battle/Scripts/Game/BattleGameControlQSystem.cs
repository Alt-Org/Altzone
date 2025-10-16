/// @file BattleGameControlQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Game,BattleGameControlQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which controls the overall game flow in %Quantum simulation.
/// </summary>

//#define DEBUG_LOG_STATE

using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

using Battle.QSimulation.Player;
using Battle.QSimulation.SoulWall;

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// <span class="brief-h">%Game control <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Controls the overall game flow in %Quantum simulation.
    /// </summary>
    ///
    /// Initializes BattleGridManager and BattlePlayerManager.<br/>
    /// Registers players to BattlePlayerManager when they connect.<br/>
    /// Controls game state transitions from initialization to active gameplay.
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

        /// <summary>
        /// Called when the game ends. Updates the game session state and calls the BattleViewGameOver Event and BattleOnGameOver Signal.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="winningTeam">The team that won the game.</param>
        /// <param name="projectile">Pointer reference to the projectile.</param>
        /// <param name="projectileEntity">The projectile entity.</param>
        public static void OnGameOver(Frame f, BattleTeamNumber winningTeam, BattleProjectileQComponent* projectile, EntityRef projectileEntity)
        {
            BattleGameSessionQSingleton* gameSession = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();
            f.Events.BattleViewGameOver(winningTeam, gameSession->GameTimeSec);
            gameSession->State = BattleGameState.GameOver;

            BattleTeamNumber WinningTeam = winningTeam;
            f.Signals.BattleOnGameOver(WinningTeam, projectile, projectileEntity);
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
