using Battle.Scripts.Battle.Ball;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class BallTrackerTest : MonoBehaviour
    {
        public bool _isPauseOnNoMansLand;

        private void OnEnable()
        {
            this.Subscribe<BallMoved>(OnBallMoved);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnBallMoved(BallMoved data)
        {
            Debug.Log($"{data}");
            if (!_isPauseOnNoMansLand)
            {
                return;
            }
            if (data.IsOnBlueTeamArea || data.IsOnRedTeamArea)
            {
                return;
            }
            UnityEngine.Debug.Break();
        }
    }
}