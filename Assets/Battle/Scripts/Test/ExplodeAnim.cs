using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using UnityEngine;

public class ExplodeAnim : MonoBehaviour
{
    [SerializeField] private GameObject _explotion;
    [SerializeField] private GameObject _hit;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Instantiate(_hit, transform.position, transform.rotation);
            Instantiate(_explotion, Context.GetGridManager.GridPositionToWorldPoint(new GridPos(transform.position.y < 0 ? 4 : 43, 13)), transform.rotation);
        }
    }
}
