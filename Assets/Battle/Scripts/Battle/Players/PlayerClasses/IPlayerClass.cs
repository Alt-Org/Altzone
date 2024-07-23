
namespace Battle.Scripts.Battle.Players
{
    interface IPlayerClass
    {
        public bool BounceOnBallShieldCollision { get; }

        /// <summary>
        /// Called when ball collides with shield before bounce.
        /// </summary>
        public void OnBallShieldCollision();

        /// <summary>
        /// Called after ball bounces from shield. <br></br>
        /// (Note that this is not called if <c>OnBallShieldCollision</c> tells ball to not bounce)
        /// </summary>
        public void OnBallShieldBounce();
    }
}
