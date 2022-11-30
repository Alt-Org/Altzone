using UnityEngine;

public class BallHandlerTest : MonoBehaviour
{
    [SerializeField] private Vector2 startingSpeed;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        LaunchBall();
    }

    private void LaunchBall()
    {
        rb.velocity = startingSpeed;
    }
}
