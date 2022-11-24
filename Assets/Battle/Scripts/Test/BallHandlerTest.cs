using UnityEngine;

public class BallHandlerTest : MonoBehaviour
{
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 startingSpeed;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.position = startPosition;
        LaunchBall();
    }

    private void LaunchBall()
    {
        rb.velocity = startingSpeed;
    }
}
