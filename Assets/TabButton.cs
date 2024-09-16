using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public TabGroup tabGroup;

    public Image background;

    // Start is called before the first frame update
    void Start()
    {
        background = GetComponent<Image>();
        tabGroup.Subscribe(this);
    }

public void OnPointerClick(PointerEventData eventData) => throw new System.NotImplementedException();
public void OnPointerEnter(PointerEventData eventData) => throw new System.NotImplementedException();
public void OnPointerExit(PointerEventData eventData) => throw new System.NotImplementedException();

}
