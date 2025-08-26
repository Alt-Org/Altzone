//#define DEBUG_LOG_STATE

using UnityEngine.Scripting;
using Quantum;

using Battle.QSimulation.Player;
using Battle.QSimulation.SoulWall;

namespace Battle.QSimulation.Game
{
    /**
     *  Systems that monitor game state:
     *  -ProjectileSpawnerSystem
     */
    [Preserve]
    public unsafe class BattleGameControlQSystem : SystemMainThread, ISignalOnPlayerAdded
    {
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

        public void OnPlayerAdded(Frame f, PlayerRef playerRef, bool firstTime)
        {
            BattlePlayerManager.RegisterPlayer(f, playerRef);
        }

        public static void OnGameOver(Frame f, BattleTeamNumber winningTeam, BattleProjectileQComponent* projectile, EntityRef projectileEntity)
        {
            BattleGameSessionQSingleton* gameSession = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();
            f.Events.BattleViewGameOver(winningTeam, gameSession->GameTimeSec);
            gameSession->State = BattleGameState.GameOver;

            BattleTeamNumber WinningTeam = winningTeam;
            f.Signals.BattleOnGameOver(WinningTeam, projectile, projectileEntity);
        }

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
                        f.Events.BattleViewInit();
                        gameSession->State = BattleGameState.CreateMap;
                    }
                    break;

                case BattleGameState.CreateMap:
                    CreateMap(f);
                    f.Events.BattleViewActivate();
                    gameSession->State = BattleGameState.Countdown;
                    break;

                // Countdown state handling
                case BattleGameState.Countdown:
                    gameSession->TimeUntilStart -= f.DeltaTime;

                    // Transition from Countdown to GetReadyToPlay
                    if (gameSession->TimeUntilStart < 1)
                    {
                        f.Events.BattleViewGetReadyToPlay();
                        gameSession->State = BattleGameState.GetReadyToPlay;
                        gameSession->TimeUntilStart = 1; // Set 1 second for the GetReadyToPlay state
                    }
                    break;

                // GetReadyToPlay state handling
                case BattleGameState.GetReadyToPlay:
                    gameSession->TimeUntilStart -= f.DeltaTime;

                    // Transition from GetReadyToPlay to Playing
                    if (gameSession->TimeUntilStart <= 0)
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
