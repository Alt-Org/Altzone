using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldTriggerOnTest : MonoBehaviour
{
    private GameObject _parent;
    private string _ballLayerMask = "Ball";
    private string _ballTag = "Ball";
    private Collider2D _shieldTriggerOn;

    public bool isTouching => _shieldTriggerOn.IsTouchingLayers(LayerMask.GetMask(_ballLayerMask));

    private void Awake()
    {
        _parent = transform.parent.gameObject;
        _shieldTriggerOn = GetComponent<Collider2D>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == _ballTag)
        {
            _parent.GetComponent<PlayerMovementTest>().TurnShieldOn();
        }
    }
}
