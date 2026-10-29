using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleWithAnchor : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject topAnchor;
    [SerializeField] private GameObject bottomAnchor;
    [SerializeField] private GameObject leftAnchor;
    [SerializeField] private GameObject rightAnchor;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        //ChangeScale();
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("on drop");
        if (eventData.pointerDrag != null)
        {
            ChangeScale();
        }
    }

    void ChangeScale()
    {
        float distanceX = Vector2.Distance(leftAnchor.transform.position, rightAnchor.transform.position);
        float distanceY = Vector2.Distance(topAnchor.transform.position, bottomAnchor.transform.position);

        
    }
}
