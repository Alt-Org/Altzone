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
            int? actorNumber = f.PlayerToActorId(player);
            

            //creates player entity from prefab and gives it playerdata component
            RuntimePlayer data = f.GetPlayerData(player);
            EntityPrototype entityPrototypeAsset = f.FindAsset(data.PlayerAvatar);
            EntityRef playerEntity = f.Create(entityPrototypeAsset);
            f.Add(playerEntity, new PlayerData{Player = player, Speed = 20, TargetPosition = default});

            //gets the right spawnpoint for player, will be replaced with playerpositions from lobby
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

            //teleports player to spawnpoint's position
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
            PlayerData* playerData = f.Unsafe.GetPointer<PlayerData>(playerEntity);

            playerTransform->Teleport(f, _spawnPos2D);
            playerData->TargetPosition = playerTransform->Position;
        }
    }
}
