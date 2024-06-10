using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Resize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Resize()
    {
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        RectTransform rectTransform = GetComponent<RectTransform>();
        var rect = rectTransform.rect;
        boxCollider2D.size = new Vector2(rect.width, rect.height);

        Vector2 offset = new Vector2(rect.width/2 - rect.width*rectTransform.pivot.x, rect.height/2 - rect.height * rectTransform.pivot.y);

        boxCollider2D.offset = offset;
    }

}
