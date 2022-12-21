using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerActorBase : MonoBehaviour
    {
        public static IPlayerActor InstantiatePrefabFor(IPlayerDriver playerDriver, PlayerActorBase playerPrefab)
        {
            var playerPos = playerDriver.PlayerPos;
            var instantiationGridPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(playerPos);
            var instantiationPosition = Context.GetGridManager.GridPositionToWorldPoint(instantiationGridPosition);
            PlayerActorBase playerActorBase;
            playerActorBase = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            var playerActor = (IPlayerActor)playerActorBase;
            return playerActor;
        }
    }
}