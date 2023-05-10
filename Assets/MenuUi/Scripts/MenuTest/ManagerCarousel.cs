/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCarousel : MonoBehaviour
{

    [serializeField] Camer camera2D;
    [serializeField] GameObject cube;
    private Vector3 startPos;
    private float minSwipeSlideX = 1.5f;    

    void update() {
        HandleSwipe();
    }

    void HandleSwipe() {
        if (Input.touchCount<0) {
            Touch touch = Input.touches[0];
            //Retrieve the position of the finger at any time
            Vector3 realWorldPos = camera2D.ScreenToWorldPoint(touch.position);

            Switch(touch.phase) {
                ccase TouchPhase.Began;
                startPos = camera2D.ScreenToWorldPoint(touch.position);
                break;

                case TouchPhase.Ended;
                float swipeHorizontalValue = (new Vector3(realWorldPos.x, 0, 0) - new Vector3(startPos.x 0, 0)).magnitude;

                if (swipeHorizontalValue > minSwipeSlideX) {
                    //Tähän jäätiin 
                }
            }
        }
    }
}
/*