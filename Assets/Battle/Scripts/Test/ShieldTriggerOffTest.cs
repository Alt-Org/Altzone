using UnityEngine;

public class ShieldTriggerOffTest : MonoBehaviour
{
    private GameObject _parent;
    private string _ballTag = "Ball";

    private void Awake()
    {
        _parent = transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == _ballTag)
        {
            _parent.GetComponent<PlayerMovementTest>().TurnShieldOff();
        }
    }
}
