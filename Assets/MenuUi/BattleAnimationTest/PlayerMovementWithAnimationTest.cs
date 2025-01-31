using Prg.Scripts.Common;
using UnityEngine;

public class PlayerMovementWithAnimationTest : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private float _moveSpeed;

    private Vector3 _targetPosition;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void Update()
    {
        if (ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move) 
        {
            _targetPosition = Camera.main.ScreenToWorldPoint(ClickStateHandler.GetClickPosition());
            _targetPosition.y = 0;
        }

        if (transform.position != _targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
            float xMovement = (_targetPosition.x - transform.position.x);
            if (xMovement != 0)
            {
                _spriteRenderer.flipX = (_targetPosition.x - transform.position.x) < 0;
            } 
            _animator.SetBool("isRunning", true);
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }
    }
}
