using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle.Players;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using Battle.Scripts.Battle;

public class DiamondMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private float _bottomBoundary;
    private float _topBoundary;
    private bool _isTopSide;

    public void Initialize(Rigidbody2D rb, float bottomBoundary, float topBoundary, bool isTopSide)
    {
        _rb = rb;
        _bottomBoundary = bottomBoundary;
        _topBoundary = topBoundary;
        _isTopSide = isTopSide;
    }

    private void Update()
    {
        if (_rb != null)
        {
            if (_isTopSide)
            {
                if (transform.position.y <= _bottomBoundary)
                {
                    _rb.velocity = Vector2.zero;
                }
            }
            else
            {
                if (transform.position.y >= _topBoundary)
                {
                    _rb.velocity = Vector2.zero;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (_rb != null)
        {
            Vector2 reflectVelocity = Vector2.Reflect(_rb.velocity, collision.contacts[0].normal);
            _rb.velocity = reflectVelocity;
        }
    }
}
