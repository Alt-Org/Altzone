using Photon.Deterministic;
using Quantum.Prototypes;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerSpawnSystem  : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {

            FPVector2[] spawnPoints = new FPVector2[]
            {
                new(-2, -5),
                new(2, -5),
                new(2, 5),
                new(-2, 5)
            };

            //gets spawnpoint for player depending on PlayerRef, will be replaced with playerpositions from lobby
            FPVector2 spawnPos2D = spawnPoints[player];

            //creates player entity from prefab and gives it playerdata component
            RuntimePlayer data = f.GetPlayerData(player);
            EntityPrototype entityPrototypeAsset = f.FindAsset(data.PlayerAvatar);
            EntityRef playerEntity = f.Create(entityPrototypeAsset);
            f.Add(playerEntity, new PlayerData{Player = player, Speed = 20, TargetPosition = spawnPos2D});

            //teleports player to spawnpoint's position
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
            playerTransform->Teleport(f, spawnPos2D);

            //rotates BetaTeam players
            if (player==2 || player==3){
                playerTransform->Rotation = FP.Rad_180;
            }
        }
    }
}
