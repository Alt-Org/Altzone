namespace Battle.Scripts.Battle
{
    internal interface IPlayerDriver
    {
        void Rotate(float angle);

        void SetCharacterPose(int poseIndex);
    }
}
