using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle.Scripts.Test
{
    public class BallControllerTest : MonoBehaviour
    {
        [SerializeField] private float _speedMultiplier;
        private GameObject _ball;
        private Camera _camera;
        private Rigidbody2D _rb;
        private Vector2 _targetPosition;

        private void Awake()
        {
            _ball = FindObjectOfType<BallHandler>().gameObject;
            _camera = Context.GetBattleCamera;
            _rb = _ball.GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _targetPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var direction = _targetPosition - _rb.position;
                _rb.velocity = direction.normalized * _speedMultiplier;
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                var _ballPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                _rb.position = _ballPosition;
                _rb.velocity = Vector3.zero;
            }
        }


    }
}
