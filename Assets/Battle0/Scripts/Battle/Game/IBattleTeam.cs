using UnityEngine;

namespace Battle0.Scripts.Battle.Game
{
    /// <summary>
    /// Interface for <c>BattleTeam</c> for managing team of one or two players internally for gameplay purposes.
    /// </summary>
    internal interface IBattleTeam
    {
        int TeamNumber { get; }
        IPlayerDriver FirstPlayer { get; }
        IPlayerDriver SecondPlayer { get; }
        int PlayerCount { get; }
        IPlayerDriver GetMyTeamMember(int actorNumber);
        int Attack { get; }
        
        void SetPlayMode(BattlePlayMode playMode);
    }
    
    internal static class BattleTeamExtensions
    {
        public static void GetBallDropPositionAndDirection(this IBattleTeam startTeam, out Vector2 ballDropPosition, out Vector2 direction)
        {
            if (startTeam == null)
            {
                do
                {
                    direction = Random.insideUnitCircle;
                } while (direction.x == 0 || direction.y == 0);

                ballDropPosition = Vector2.zero;
                return;
            }
            var center = Vector3.zero;

            var transform1 = startTeam.FirstPlayer.PlayerTransform;
            var pos1 = transform1.position;
            if (startTeam.PlayerCount == 1)
            {
                direction = center - pos1;
                ballDropPosition = pos1;
                return;
            }
            var transform2 = startTeam.SecondPlayer.PlayerTransform;
            var pos2 = transform2.position;
            var dist1 = Mathf.Abs((pos1 - center).sqrMagnitude);
            var dist2 = Mathf.Abs((pos2 - center).sqrMagnitude);
            if (dist1 < dist2)
            {
                direction = pos1 - pos2;
                ballDropPosition = pos1;
            }
            else
            {
                direction = pos2 - pos1;
                ballDropPosition = pos2;
            }
        }
    }
}