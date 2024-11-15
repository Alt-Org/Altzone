using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Get the wall script component and trigger the hit
            BreakableWall wall = collision.gameObject.GetComponent<BreakableWall>();
            if (wall != null)
            {
                wall.OnWallHit();
            }
        }
    }
}
