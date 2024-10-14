using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Prg.Scripts.Common;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;

public class ImageColorChanger : MonoBehaviour
{
    public Color selectedColor = Color.white;   //  default Color
    public GraphicRaycaster raycaster;  
    public EventSystem eventSystem;

    [SerializeField]
    private Transform _heart;

    void Start()
    {

        EnhancedTouchSupport.Enable();
        if (raycaster == null)
        {
            raycaster = FindObjectOfType<GraphicRaycaster>();
        }

        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }
        int i = 0;
        foreach (Transform heartPiece in _heart)
        {
            heartPiece.GetComponent<HeartPieceColorHandler>().Initialize(i);
            i++; 
        }

    }

 //color Change Colors
    public void SetColorGreen()
    {
        selectedColor = Color.green;
        Debug.Log("Color selected: Green");
    }

    public void SetColorRed()
    {
        selectedColor = Color.red;
        Debug.Log("Color selected: Red");
    }
    public void SetColorWhite()
    {
        selectedColor = Color.white;
        Debug.Log("Color selected: White");
    }
    public void SetColorMagenta()
    {
        selectedColor = Color.magenta;
        Debug.Log("Color selected: Magenta");
    }
    public void SetColorBlue()
    {
        selectedColor = Color.blue;
        Debug.Log("Color selected: Blue");
    }

    public void SetColorYellow()
    {
        selectedColor = Color.yellow;
        Debug.Log("Color selected: Yellow ");
    }

   


    void Update()
    {
        if (ClickStateHandler.GetClickState() == ClickState.Start)  
        {
            Vector2 currentPosition = new();
            if (Touch.activeTouches.Count == 1) currentPosition = Touch.activeFingers[0].screenPosition;
            else if(Mouse.current != null)currentPosition = Mouse.current.position.ReadValue();
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = currentPosition
            };

          
            List<RaycastResult> results = new List<RaycastResult>();

          
            raycaster.Raycast(pointerData, results);

     
            foreach (RaycastResult result in results)
            {

                HeartPieceColorHandler clickedImage = result.gameObject.GetComponent<HeartPieceColorHandler>();

                // Tag for color change
                if (clickedImage != null && result.gameObject.CompareTag("HeartPiece"))
                {
                    // Change the color of the Image component
                    clickedImage.SetColor (selectedColor);
                    Debug.Log("Changed color of " + result.gameObject.name + " to " + selectedColor);
                }
                else
                {
                    // Debug.log/console.log For wrong position click
                    Debug.Log("Object " + result.gameObject.name + " is not a heart piece or has no Image component.");
                }
            }
        }
    }
}
