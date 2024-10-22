using UnityEngine;

namespace Battle1.Scripts.Test.Elmeri
{
    public class BoxcolliderTest : MonoBehaviour
    {
        [SerializeField] BoxCollider2D platformCollider;
        [SerializeField] SpriteRenderer spriteRenderer;

        private void Update()
        {
            platformCollider.size = new Vector2(spriteRenderer.size.x, spriteRenderer.size.y);
        }
    }
}
