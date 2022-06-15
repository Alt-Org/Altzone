using Battle.Scripts.Battle.Ball;
using Battle.Scripts.Battle.Game;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Service locator pattern for important objects in this game.
    /// </summary>
    internal static class Context
    {
        #region Static Gameplay

        internal static IBattleCamera GetBattleCamera => Object.FindObjectOfType<GameCamera>();

        internal static IBattleBackground GetBattleBackground => Object.FindObjectOfType<GameBackground>();

        internal static IBattlePlayArea GetBattlePlayArea => Object.FindObjectOfType<PlayerPlayArea>();

        #endregion

        #region Static Actors

        public static IBallManager BallManager => Object.FindObjectOfType<BallManager>();

        #endregion

        #region Dynamic Actors

        public static IGameplayManager GameplayManager => Object.FindObjectOfType<GameplayManager>();

        
        #endregion
    }
}