using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerJoinSystem  : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            // TODO: BattleArenaSpec PlayerSpawnPositions should be used instead
            FPVector2[] spawnPoints = new FPVector2[]
            {
                new(-2, -5),
                new( 2, -5),
                new( 2,  5),
                new(-2,  5)
            };

            RuntimePlayer data = f.GetPlayerData(player);
            BattlePlayerSlot playerSlot = (BattlePlayerSlot)data.PlayerSlot;
            FPVector2 spawnPos2D = spawnPoints[PlayerManager.GetPlayerIndex(playerSlot)];

            PlayerManager.InitPlayer(f, player);
            PlayerManager.SpawnPlayer(f, playerSlot, 0, spawnPos2D);

            f.Events.UpdateDebugStatsOverlay(data.Characters[0]);
        }

    }
}
