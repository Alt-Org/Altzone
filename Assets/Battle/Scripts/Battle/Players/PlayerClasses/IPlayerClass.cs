using System;

namespace Battle.Scripts.Battle.Players
{
    interface IPlayerClass
    {
        /// <summary>
        /// <b>SpecialAbilityOverridesBallBounce is deprecated, please use return value of OnBallShieldCollision instead.</b><br></br>
        /// <br></br>
        /// False: Ball bounces normaly before activating special ability. <br></br>
        /// True: Ball does not bounce.
        /// </summary>
        [Obsolete("SpecialAbilityOverridesBallBounce is deprecated, please use return value of OnBallShieldCollision instead.")]
        public bool SpecialAbilityOverridesBallBounce { get; }

        /// <summary>
        /// Called when ball collides with shield before bounce.
        /// </summary>
        /// <returns> Should ball bounce (true or false) </returns>
        public bool OnBallShieldCollision();

        /// <summary>
        /// Called after ball bounces from shield. <br></br>
        /// (Note that this is not called if OnBallShieldCollision tells ball to not bounce)
        /// </summary>
        public void OnBallShieldBounce();

        /// <summary>
        /// <b>ActivateSpecialAbility is deprecated, please use OnBallShieldCollision and/or OnBallShieldBounce instead.</b><br></br>
        /// <br></br>
        /// Activates special ability. <br></br>
        /// (called when ball hits shield)
        /// </summary>
        [Obsolete("ActivateSpecialAbility is deprecated, please use OnBallShieldCollision and/or OnBallShieldBounce instead.")]
        public void ActivateSpecialAbility();
    }
}
