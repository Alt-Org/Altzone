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

    private float _time = 0;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _targetPosition = transform.position;
    }

    private void Update()
    {
        if (ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move)
        {
            _targetPosition = Camera.main.ScreenToWorldPoint(ClickStateHandler.GetClickPosition());
            _targetPosition.y = 0;

            _time = 0;
        }

        int animationState = 0;
        bool flipX = false;

        if (_time >= 2.5f)
        {
            animationState = 3;
        }

        else if (transform.position != _targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
            Vector3 movement = _targetPosition - transform.position;

            if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.z) * 0.25)
            {
                flipX = movement.x < 0;
                animationState = 1;
            }
            else
            {
                animationState = 2;
            }
        }

        _spriteRenderer.flipX = flipX;
        _animator.SetInteger("state", animationState);

        _time += Time.deltaTime;
    }
}
