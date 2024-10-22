using System;
using UnityEngine;

namespace Battle1.Scripts.Battle.Players
{
    public class BotInputHandler : MonoBehaviour
    {
        [Header("Debug Settings")] public GameObject _hostForInput;

        private Transform _ballTransform;
        private Transform _botTransform;

        private Action<Vector2> _onMoveTo;
        public Action<Vector2> OnMoveTo
        {
            set
            {
                _onMoveTo = value;
            }
        }

        private Vector2 _targetPosition;

        // We might want to simulate mobile device screen by ignoring click outside out window.
        private bool _isLimitMouseXYOnDesktop;

        private void Awake()
        {
            _isLimitMouseXYOnDesktop = AppPlatform.IsDesktop;
        }


        private void FixedUpdate()
        {
            if (_hostForInput == null)
            {
                _hostForInput = GameObject.FindGameObjectWithTag("PlayerDriverPhoton");

            }
            if (_botTransform == null)
            {
                _botTransform = GameObject.Find($"PlayerActor0({_hostForInput.GetComponent<PlayerDriverPhoton>().PlayerName})").transform;
            }
            if (_ballTransform == null)
            {
                _ballTransform = GameObject.Find("SyncedBall").transform;
            }

            FindMovePosition();
        }

        private void FindMovePosition()
        {
            _targetPosition = new Vector2(_ballTransform.position.x + UnityEngine.Random.Range(-0.5f, 0.5f)
            , _botTransform.position.y + UnityEngine.Random.Range(-1f, 1f));
            _hostForInput.GetComponent<PlayerDriverPhoton>().MoveTo(_targetPosition);
            return;
        }
    }
}

