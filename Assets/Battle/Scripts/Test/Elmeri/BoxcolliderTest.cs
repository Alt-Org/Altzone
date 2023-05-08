using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxcolliderTest : MonoBehaviour
{
    [SerializeField] BoxCollider2D platformCollider;
    [SerializeField] SpriteRenderer spriteRenderer;

    private void Update()
    {
        platformCollider.size = new Vector2(spriteRenderer.size.x, spriteRenderer.size.y);
    }
}
