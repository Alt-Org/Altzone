using Prg.Scripts.Common.PubSub;
using Battle.Scripts.Battle.Game;
using UnityEngine;
using UnityEngine.Assertions;
using Battle.Scripts.Battle;

namespace Battle.Scripts.Ui
{
    public class SlingshotTrajectory : MonoBehaviour
    {

        [Header("Line"), SerializeField] private GameObject _lineObject;
        private LineRenderer _line;
        [SerializeField] private Transform _playerTransform;
        private bool _playerIsOn = false;


        private void Awake()
        {
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

        private void LateUpdate()
        {
            // This'll need to be improved upon, and might be replaced with a coroutine
            if(_playerIsOn)
            {
                _line.SetPosition(0, _playerTransform.position);
            }
        }

        private void SlingStart(UiEvents.SlingshotStart start)
        {
            // This needs to be re-done, This only works if there is at least a player
            // that occupies the first position on the list
            _playerTransform = start.TeamTrackers[0].Player1Transform;
            _playerIsOn = true;
            _lineObject.SetActive(true);
        }

        private void SlingEnd(UiEvents.SlingshotEnd end)
        {
            _playerIsOn = false;
            _lineObject.SetActive(false);
        }
    }
}
