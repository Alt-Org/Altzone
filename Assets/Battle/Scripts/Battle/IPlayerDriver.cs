namespace Battle.Scripts.Battle
{
    internal interface IPlayerDriver
    {
        string NickName { get; }

        int TeamNumber { get; }

        int ActorNumber { get; }

        bool IsLocal { get; }

        int PlayerPos { get; }

        void Rotate(float angle);

        void SetCharacterPose(int poseIndex);
    }
}
