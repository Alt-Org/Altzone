using System.Collections;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Ui
{
    internal class SlingshotTrajectory : MonoBehaviour
    {
        [Header("Team Settings"), SerializeField] private int _teamNumber;

        [Header("Line Settings"), SerializeField] private GameObject _lineObject;
        [SerializeField] private float _lineLengthMultiplier = 2.0f;
        
        private LineRenderer _line;
        private bool _playerIsOn;
        private IPlayerManager _playerManager;

        private void Awake()
        {
            Debug.Log($"{name} team {_teamNumber}");
            _playerManager = FindObjectOfType<PlayerManager>();
            Assert.IsNotNull(_playerManager, "_playerManager != null");
            _line = _lineObject.GetComponent<LineRenderer>();
            Assert.IsNotNull(_line, "_line != null");
            _line.SetPosition(0, new Vector2(0f, 0f));
            _line.SetPosition(1, new Vector2(0f, 0f));
        }

        private void OnEnable()
        {
            this.Subscribe<UiEvents.SlingshotStart>(SlingStart);
            this.Subscribe<UiEvents.SlingshotEnd>(SlingEnd);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private IEnumerator UpdateSling(IBattleTeam battleTeam)
        {
            _playerIsOn = true;
            _lineObject.SetActive(true);

            while (_playerIsOn)
            {
                // Because of how this is setup, the line of that shows where the ball will be shot,
                // changes according to which player will shoot the ball according to the conditions
                // set within the GetBallDropPositionAndDirection()
                battleTeam.GetBallDropPositionAndDirection(out var dropPosition, out var direction);
                _line.SetPosition(0, dropPosition);
                _line.SetPosition(1, dropPosition + (direction * _lineLengthMultiplier));
                yield return null;
            }

            _lineObject.SetActive(false);
        }

        private void SlingStart(UiEvents.SlingshotStart start)
        {
            var team = start.TeamTrackers.Find(x => x.TeamNumber == _teamNumber);
            Debug.Log($"{name} team {_teamNumber} found {team}");
            if (team == null || team.PlayerCount == 0)
            {
                return;
            }
            var battleTeam = _playerManager.GetBattleTeam(_teamNumber);
            StartCoroutine(UpdateSling(battleTeam));
        }

        private void SlingEnd(UiEvents.SlingshotEnd end)
        {
            _playerIsOn = false;
        }
    }
}