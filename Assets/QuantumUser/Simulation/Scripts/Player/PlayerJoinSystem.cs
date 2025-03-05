using System.Data.Common;
using Photon.Deterministic;
using Quantum.Prototypes;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerJoinSystem  : SystemSignalsOnly, ISignalOnPlayerAdded
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

       /*     BattleCharacterBase character;*/
            // Stat overrides
            //character.Attack = 10;

            //gets spawnpoint for player depending on PlayerRef, will be replaced with playerpositions from lobby
            FPVector2 spawnPos2D = spawnPoints[player];
            FP rotation;
            FPVector2 normal;

            if (player==0 || player==1)
            {
                rotation = 0;
                normal = new FPVector2(0,1);
            }
            else
            {
                rotation = 180;
                normal = new FPVector2(0,-1);
            }


            //creates player entity from prefab and gives it playerdata component
            RuntimePlayer data = f.GetPlayerData(player);
            EntityPrototype entityPrototypeAsset = f.FindAsset(data.PlayerAvatar);
            EntityRef playerEntity = f.Create(entityPrototypeAsset);
            f.Add(playerEntity, new PlayerData { Player = player, TargetPosition = spawnPos2D, Rotation = 0, Normal = normal, CollisionMinOffset = 1, Speed = 20 });

            //set position and rotation
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
            playerTransform->Teleport(f, spawnPos2D);
            playerTransform->Rotation = rotation;

            f.Events.UpdateDebugStatsOverlay(data.Characters[0]);
        }
    }
}
