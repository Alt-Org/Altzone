namespace Battle.Scripts.Battle.Players
{
    interface IPlayerClass
    {
        /// <summary>
        /// False: Ball bounces normaly before activating special ability <br></br>
        /// True: Ball does not bounce 
        /// </summary>
        public bool SpecialAbilityOverridesBallBounce { get; }

        /// <summary>
        /// Activates special ability. <br></br>
        /// (called when ball hits shield)
        /// </summary>
        public void ActivateSpecialAbility();
    }
}
