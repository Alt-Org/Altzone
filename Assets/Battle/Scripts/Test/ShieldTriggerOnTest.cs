using UnityEngine;
using UnityConstants;

public class ShieldTriggerOnTest : MonoBehaviour
{
    private GameObject _parent;
    private Collider2D _shieldTriggerOn;

    public bool isTouching => _shieldTriggerOn.IsTouchingLayers(LayerMask.GetMask(Tags.Ball));

    private void Awake()
    {
        _parent = transform.parent.gameObject;
        _shieldTriggerOn = GetComponent<Collider2D>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var otherGameObject = collision.gameObject;
        if (otherGameObject.CompareTag(Tags.Ball))
        {
            _parent.GetComponent<PlayerMovementTest>().TurnShieldOn();
        }
    }
}
