using System.Collections;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;

using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Ui
{
    internal class SlingshotTrajectory : MonoBehaviour
    {

        [Header("Line"), SerializeField] private GameObject _lineObject;
        private LineRenderer _line;
        private Transform[] _playerTransform = new Transform[2];
        private bool _playerIsOn = false;
        private IPlayerManager _playerManager;


        private void Awake()
        {
            _playerManager = FindObjectOfType<PlayerManager>();
            _line = _lineObject.GetComponent<LineRenderer>();
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

        private IEnumerator UpdateSling(int num)
        {
            IBattleTeam startTeam = _playerManager.GetBattleTeam(num);
            _playerIsOn = true;

            _lineObject.SetActive(true);

            while (_playerIsOn)
            {
                // Because of how this is setup, the line of that shows where the ball will be shot,
                // changes according to which player will shoot the ball according to the conditions
                // set within the GetBallDropPositionAndDirection()
                startTeam.GetBallDropPositionAndDirection(out var ball, out var dir);
                _line.SetPosition(0, ball);
                _line.SetPosition(1, ball+dir);
                yield return null;
            }

            _lineObject.SetActive(false);
        }
            

        private void SlingStart(UiEvents.SlingshotStart start)
        {
            StartCoroutine(UpdateSling(start.TeamTrackers[0].TeamNumber));
        }

        private void SlingEnd(UiEvents.SlingshotEnd end)
        {
            _playerIsOn = false;
            
        }
    }
}
