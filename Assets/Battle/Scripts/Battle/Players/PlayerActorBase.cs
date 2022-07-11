using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerActorBase : MonoBehaviour
    {
        protected static readonly char[] PlayerPosChars = { '?', 'a', 'b', 'c', 'd' };
        protected static readonly char[] PlayModes = { 'n', 'F', 'g', 'S', 'R', 'o', '-' };

        public static IPlayerActor InstantiatePrefabFor(IPlayerDriver playerDriver, PlayerActor playerPrefab, Defence defence)
        {
            var playerPos = playerDriver.PlayerPos;
            var instantiationPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(playerPos);

            PlayerActorBase playerActorBase;
            IPlayerActor playerActor;
            if (playerPrefab == null)
            {
                // This must be PlayerActor2 which must be obtained via RuntimeGameConfig!
                var runtimeGameConfig = RuntimeGameConfig.Get();
                var playerPrefab2 = runtimeGameConfig.Prefabs.GetPlayerPrefab(defence);
                Debug.Log($"defence {defence} prefab {playerPrefab2.name}");
                var playerInstance = Instantiate(playerPrefab2, instantiationPosition, Quaternion.identity);
                playerActorBase = playerInstance.GetComponent<PlayerActor2>();
                playerActor = (IPlayerActor)playerActorBase;
            }
            else
            {
                // Simpler test prefab given us directly.
                Debug.Log($"prefab {playerPrefab.name}");
                playerActorBase = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
                playerActor = (IPlayerActor)playerActorBase;
            }
            var playerTag = $"{playerPos}:{playerDriver.NickName}:{PlayerPosChars[playerPos]}";
            playerActorBase.name = playerActorBase.name.Replace("Clone", playerTag);
            playerActorBase.SetPlayerDriver(playerDriver);
            return playerActor;
        }

        protected virtual void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            throw new NotImplementedException();
        }
    }
}