using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle.Bot
{
    internal class BotActor : MonoBehaviour
    {
        [SerializeField] Transform ballTransform;
        private float _movementSpeed = 5;
        private Vector2 targetPosition;
        private bool _hasTarget;

        private void FindMovePosition()
        {
            if(_hasTarget == false)
            {
                targetPosition = new Vector2(ballTransform.position.x + Random.Range(-2.0f,2.0f), transform.position.y);
                StartCoroutine(MoveCoroutine(targetPosition));
            }
        }

        private void Update()
        {
            FindMovePosition();
        }

        private IEnumerator MoveCoroutine(Vector2 position)
        {
            Vector3 targetPosition = position;
            _hasTarget = true;
            while (_hasTarget)
            {
                yield return null;
                var maxDistanceDelta = _movementSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, position, maxDistanceDelta);
                _hasTarget = !(Mathf.Approximately(transform.position.x, position.x) && Mathf.Approximately(transform.position.y, position.y));
            }
        }
    }
}