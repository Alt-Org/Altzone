using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerSpawnSystem  : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        private FPVector2 _spawnPos2D;

        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            //creates player entity from prefab and gives it playerdata component
            RuntimePlayer data = f.GetPlayerData(player);
            EntityPrototype entityPrototypeAsset = f.FindAsset(data.PlayerAvatar);
            EntityRef playerEntity = f.Create(entityPrototypeAsset);
            f.Add(playerEntity, new PlayerData{Player = player, Speed = 20, CurrentPos = default, TargetPos = default, TargetPos2D = default, isAllowedToMove = false});

            //gets the right spawnpoint for player
            int spawnCount = f.ComponentCount<SpawnIdentifier>();
            if (spawnCount != 0)
            {
                int playerID = player;
                int i = 0;
                foreach (var (spawn, spawnIdentifier) in f.Unsafe.GetComponentBlockIterator<SpawnIdentifier>())
                {
                    if (i == playerID)
                    {
                        Transform2D spawnTransform = f.Get<Transform2D>(spawn);
                        _spawnPos2D = spawnTransform.Position;
                        break;
                    }

                i++;
                }
            }

            //teleports player to spawnpoint's position and updates current player position
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
            PlayerData* playerData = f.Unsafe.GetPointer<PlayerData>(playerEntity);

            playerTransform->Teleport(f, _spawnPos2D);
            playerData->CurrentPos = playerTransform->Position;
        }
    }
}
