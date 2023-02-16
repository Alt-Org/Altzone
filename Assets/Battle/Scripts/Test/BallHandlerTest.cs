using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BallHandlerTest : MonoBehaviour
{
    [SerializeField] private int startingSpeed;
    private Transform _transform;
    private const float waitTime = 2f;

    private Rigidbody2D rb;

    private void Start()
    {
        _transform = transform;
        rb = GetComponent<Rigidbody2D>();     
        StartCoroutine(LaunchBall());
    }

    private IEnumerator LaunchBall()
    {
        yield return new WaitForSeconds(waitTime);
        if (!PhotonNetwork.IsMasterClient)
        {
            yield break;
        }
        var randomDir = new Vector2(Random.Range(-4f, 4f), Random.Range(4f, 8f));
        var randomSide = Random.value;
        if (randomSide < 0.5)
        {
            rb.velocity = -startingSpeed * randomDir.normalized;
            yield break;
        }
        rb.velocity = startingSpeed * randomDir.normalized;
    }
    private void Update()
    {
        var velocity = rb.velocity;
        var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        _transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
