using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BallHandlerTest : MonoBehaviour
{
    [SerializeField] private int startingSpeed;
    private const float waitTime = 2f;

    private Rigidbody2D rb;

    private void Start()
    {
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
}
