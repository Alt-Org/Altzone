using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Common base class for <c>IPlayerActor</c> implementations.
    /// </summary>
    internal class PlayerActorBase : MonoBehaviour
    {
        public static IPlayerActor InstantiatePrefabFor(int playerPos, PlayerActorBase playerPrefab)
        {
            var instantiationGridPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(playerPos);
            var instantiationPosition = Context.GetGridManager.GridPositionToWorldPoint(instantiationGridPosition);
            var playerActorBase = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            var playerActor = (IPlayerActor)playerActorBase;
            return playerActor;
        }
    }
}
