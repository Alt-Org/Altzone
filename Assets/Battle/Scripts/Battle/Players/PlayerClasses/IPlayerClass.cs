
namespace Battle.Scripts.Battle.Players
{
    interface IPlayerClass
    {
        /// <summary>
        /// Reference to the <c>BattlePlayer</c> this player class is associated with.
        /// </summary>
        public IReadOnlyBattlePlayer BattlePlayer { get; }

        /// <summary>
        /// Whether or not ball should bounce when it collides with player shield.
        /// </summary>
        public bool BounceOnBallShieldCollision { get; }

        /// <summary>
        /// Initializes player class.<br></br>
        /// Called by <c>PlayerActor</c> when player is instantiated
        /// </summary>
        /// <param name="battlePlayer">Reference to the <c>BattlePlayer</c> this player class is associated with.</param>
        public void InitInstance(IReadOnlyBattlePlayer battlePlayer);

        /// <summary>
        /// Called when ball collides with shield before bounce.
        /// </summary>
        public void OnBallShieldCollision();

        /// <summary>
        /// Called after ball bounces from shield.<br></br>
        /// (Note that this is not called if <c>BounceOnBallShieldCollision</c> returns false)
        /// </summary>
        public void OnBallShieldBounce();
    }
}
