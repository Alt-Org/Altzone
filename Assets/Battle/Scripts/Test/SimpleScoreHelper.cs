using Altzone.Scripts.Battle;
using Battle.Scripts.Battle;
using TMPro;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class SimpleScoreHelper : MonoBehaviour
    {
        [Range(1, 2), SerializeField] private int _teamNumber;
        [SerializeField] private TMP_Text _scoreText;

        private IGameScoreManager _scoreManager;
        
        private void OnEnable()
        {
            _scoreManager = Context.GetGameScoreManager;
        }

        private void Update()
        {
            if (_teamNumber == PhotonBattle.TeamBlueValue)
            {
                var score = _scoreManager.BlueScore;
                _scoreText.text = $"{score.Item1}-{score.Item2}";
            }
            else if (_teamNumber == PhotonBattle.TeamRedValue)
            {
                var score = _scoreManager.RedScore;
                _scoreText.text = $"{score.Item1}-{score.Item2}";
            }
        }
    }
}