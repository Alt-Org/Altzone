using System.Collections;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Ui
{
    internal class SlingshotTrajectory : MonoBehaviour
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

        private IEnumerator UpdateSling()
        {
            _playerIsOn = true;
            _lineObject.SetActive(true);

            while (_playerIsOn)
            {
                _line.SetPosition(0, _playerTransform.position);
                yield return null;
            }

            _lineObject.SetActive(false);
        }
            

        private void SlingStart(UiEvents.SlingshotStart start)
        {
            // This needs to be re-done, This only works if there is at least a player
            // that occupies the first position on the list
            _playerTransform = start.TeamTrackers[0].Player1Transform;
            StartCoroutine(UpdateSling());
        }

        private void SlingEnd(UiEvents.SlingshotEnd end)
        {
            _playerIsOn = false;
            
        }
    }
}
