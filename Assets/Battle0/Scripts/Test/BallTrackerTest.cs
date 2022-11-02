using Battle0.Scripts.Ui;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle0.Scripts.Test
{
    internal class BallTrackerTest : MonoBehaviour
    {
        public bool _isPauseOnNoMansLand;

        private void OnEnable()
        {
            this.Subscribe<UiEvents.TeamActivation>(OnBallMoved);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnBallMoved(UiEvents.TeamActivation data)
        {
            Debug.Log($"{data}");
            if (!_isPauseOnNoMansLand)
            {
                return;
            }
            if (data.IsBallOnBlueTeamArea || data.IsBallOnRedTeamArea)
            {
                return;
            }
            UnityEngine.Debug.Break();
        }
    }
}