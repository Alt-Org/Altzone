using Battle.Scripts.Battle.Room;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class ScoreManagerTest : MonoBehaviour
    {
        private enum ScoreType
        {
            BadEnum = 0,
            BlueHead = Battle.interfaces.ScoreType.BlueHead,
            RedHead = Battle.interfaces.ScoreType.RedHead,
            BlueWall = Battle.interfaces.ScoreType.BlueWall,
            RedWall = Battle.interfaces.ScoreType.RedWall,
        }

        [Header("Debug Only")] public bool _addScore;

        [Header("Score"), SerializeField] private ScoreType _scoreType;
        [Min(0)] public int _scoreAmount;

        private void OnEnable()
        {
            this.Subscribe<ScoreManager.GameScoreEvent>(OnGameScoreEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void Update()
        {
            if (_addScore)
            {
                _addScore = false;
                var scoreType = (Battle.interfaces.ScoreType)_scoreType;
                this.Publish(new ScoreManager.ScoreEvent(scoreType, _scoreAmount));
            }
        }

        private void OnGameScoreEvent(ScoreManager.GameScoreEvent data)
        {
            Debug.Log($"OnGameScoreEvent {data}");
        }
    }
}