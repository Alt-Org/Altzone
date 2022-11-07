using Battle0.Scripts.Battle.Ball;
using Battle0.Scripts.Battle.Game;
using UnityEngine;

namespace Battle0.Scripts.Battle
{
    /// <summary>
    /// Service locator pattern for important objects in this game.
    /// </summary>
    public static class Context
    {
        #region Static Gameplay

        public static IBattleCamera GetBattleCamera => Object.FindObjectOfType<GameCamera>();

        internal static IBattleBackground GetBattleBackground => Object.FindObjectOfType<GameBackground>();

        public static IBattlePlayArea GetBattlePlayArea => Object.FindObjectOfType<PlayerPlayArea>();

        internal static IGameScoreManager GetGameScoreManager => Object.FindObjectOfType<GameScoreManager>();

        public static IGridManager GetGridManager => Object.FindObjectOfType<GridManager>();

        #endregion

        #region Static Actors

        internal static IBallManager BallManager => Object.FindObjectOfType<BallManager>();

        #endregion

        #region Dynamic Actors

        internal static IPlayerManager PlayerManager => Object.FindObjectOfType<PlayerManager>();

        #endregion
    }
}
