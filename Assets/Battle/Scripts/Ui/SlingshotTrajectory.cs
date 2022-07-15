using Prg.Scripts.Common.PubSub;
using Battle.Scripts.Battle.Game;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Ui
{
    public class SlingshotTrajectory : MonoBehaviour
    {

        [Header("Line"), SerializeField] private GameObject _lineObject;
        private LineRenderer _line;
        private float _distance;
        private Vector2 _startPos;
        private Vector2 _endpos = new Vector2(0,0);
        private IBattleTeam _player;
        [SerializeField] private Transform _playerTransform;


        private void Awake()
        {
            _line = _lineObject.GetComponent<LineRenderer>();
        }

        private void OnEnable()
        {
            //this.Subscribe<UiEvents.SlingshotTrackerEvent>(SlingshotTracking);
            this.Subscribe<UiEvents.SlingshotStart>(SlingStart);
            this.Subscribe<UiEvents.SlingshotEnd>(SlingEnd);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }
        /*private void SlingshotTracking(UiEvents.SlingshotTrackerEvent data)
        {

        }*/

        private void LateUpdate()
        {
            _player.GetBallDropPositionAndDirection(out var ballDropPosition, out var direction);
            _line.SetPosition(0, ballDropPosition);
        }

        private void SlingStart(UiEvents.SlingshotStart start)
        {
            _playerTransform = start.TeamTracker1.Player1Transform;
            _startPos = _playerTransform.position;
            _line.SetPosition(1, _endpos);
            _lineObject.SetActive(true);
        }

        private void SlingEnd(UiEvents.SlingshotEnd end)
        {
            _lineObject.SetActive(false);
        }

       private void GetDirAndPos()
       {

       }
    }
}
