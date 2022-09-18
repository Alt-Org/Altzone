using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Common base class for <c>IPlayerActor</c> implementations.
    /// </summary>
    /// <remarks>
    /// We have two implementations due to testing schedule.<br />
    /// Requirements for <c>PlayerActor2</c> were not available when implementation and testing started. 
    /// </remarks>
    internal class PlayerActorBase : MonoBehaviour
    {
        protected static readonly char[] PlayerPosChars = { '?', 'a', 'b', 'c', 'd' };
        protected static readonly char[] PlayModes = { 'n', 'F', 'g', 'S', 'R', 'o', '-' };

        public static IPlayerActor InstantiatePrefabFor(IPlayerDriver playerDriver, Defence defence, PlayerActorBase playerPrefabTest)
        {
            var playerPos = playerDriver.PlayerPos;
            var instantiationPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(playerPos);
            if (!RuntimeGameConfig.Get().Features._isDisableBattleGridMovement)
            {
                var gridManager = Context.GetGridManager;
                var playerGridPos = gridManager.CalcRowAndColumn(instantiationPosition, Context.GetBattleCamera.IsRotated);
                instantiationPosition = gridManager.GridPositionToWorldpoint(playerGridPos[0], playerGridPos[1], Context.GetBattleCamera.IsRotated);
            }

                PlayerActorBase playerActorBase;
            if (playerPrefabTest == null)
            {
                // This must be PlayerActor2 prefab which must be obtained via RuntimeGameConfig!
                var runtimeGameConfig = RuntimeGameConfig.Get();
                var playerPrefab = runtimeGameConfig.Prefabs.GetPlayerPrefab(defence);
                Debug.Log($"defence {defence} prefab {playerPrefab.name}");
                var playerInstance = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
                playerActorBase = playerInstance.GetComponent<PlayerActor2>();
                Assert.IsNotNull(playerActorBase, "playerActorBase != null");
            }
            else
            {
                // Test prefab given us directly.
                Debug.Log($"prefab {playerPrefabTest.name}");
                playerActorBase = Instantiate(playerPrefabTest, instantiationPosition, Quaternion.identity);
            }
            var playerActor = (IPlayerActor)playerActorBase;
            Assert.IsNotNull(playerActor, "playerActor != null");
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
