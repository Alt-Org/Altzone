namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Callback interface for <c>PlayerActor</c> to command <c>PlayerDriver</c> implementation. 
    /// </summary>
    internal interface IPlayerDriverCallback
    {
        void SetCharacterPose(int poseIndex);
    }
}
