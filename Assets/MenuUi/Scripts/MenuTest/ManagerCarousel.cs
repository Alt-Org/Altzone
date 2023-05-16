using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerCarousel : MonoBehaviour
{
    [SerializeField]
    Camera camere2D; // Reference to the 2D camera in the scene
    [SerializeField]
    Image blackBackground; // Reference to a black background image used to create a fade effect

    // Initial position of the finger touch
    private Vector3 startPos;
    private float minSwipeSlideX = 0.5f; // Minimum distance required to consider a swipe as a slide

    // List of the relative positions of each slide in the scrollbar (0 to 1)
    [SerializeField]
    private List<float> posScrollValue = new List<float>();

    // List of the slide game objects in the scene
    [SerializeField]
    private List<GameObject> mySlide = new List<GameObject>();



    private int currentSlide = 0; // Index of the current slide
    private bool isSliding; // Flag to prevent multiple slides at the same time


    [SerializeField]
    private ScrollRect scrollRect; // Reference to the ScrollRect component used to control the scrollbar
    [SerializeField]
    private Scrollbar scrollbar; // Reference to the Scrollbar component used to control the scrollbar



    private void Start()
    {
        SetUpScrollBar(); // Set up the relative positions of each slide in the scrollbar
    }



    private void Update()
    {
        HandleSwipe(); // Handle user input for sliding the carousel
    }



    // Set the visibility of the slides based on the given page index
    public void ShowPage(int pageIndex)
    {
        for (int i = 0; i < mySlide.Count; i++)
        {
            bool show = (i == pageIndex);
            mySlide[i].SetActive(show);
        }

        currentSlide = pageIndex;
    }


    // These 2 are used on buttons :D
    // Go to the next slide if possible
    public void GoToNextSlide()
    {
        if (currentSlide < mySlide.Count - 1 && !isSliding)
        {
            currentSlide++;
            CheckSlide();
        }
        else if (currentSlide == mySlide.Count - 1 && !isSliding)
    {
        // If it's the last slide, wrap around to the first slide
        currentSlide = 0;
        CheckSlide();
    }

    }

// Go to the previous slide if possible
public void GoToPreviousSlide()
{
    int previousSlide = currentSlide - 1;
    if (previousSlide < 0)
    {
        previousSlide = mySlide.Count - 1;
    }

    currentSlide = previousSlide;
    CheckSlide();
}
    // ^^

    // Handle user input for sliding the carousel
void HandleSwipe()
{
    if (Input.touchCount > 0 && isSliding == false)
    {
        Touch touch = Input.touches[0];
        // Retrieve the position of the finger in the real world coordinates
        Vector3 realWorldPos = camere2D.ScreenToWorldPoint(touch.position);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                startPos = camere2D.ScreenToWorldPoint(touch.position);
                scrollRect.enabled = false; // Disable the ScrollRect component to prevent scrolling during the slide
                break;

            case TouchPhase.Ended:
                // Calculate the horizontal distance between the initial touch position and the current touch position
                float swipeHorizontalValue = (new Vector3(realWorldPos.x, 0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;
                // Check if the swipe distance is greater than the minimum required to consider it a slide
                if (swipeHorizontalValue > minSwipeSlideX)
                {
                    // Determine the direction of the swipe based on the sign of the horizontal distance
                    float swipeValue = Mathf.Sign(realWorldPos.x - startPos.x);
                    if (swipeValue > 0)
                    {
                        // If the swipe is to the right, go to the previous slide if available
                        if (currentSlide > 0)
                        {
                            currentSlide = currentSlide - 1;
                            CheckSlide();
                        }
                    }
                    else if (swipeValue < 0)
                    {
                        // If the swipe is to the left, go to the next slide if available
                        if (currentSlide < mySlide.Count-1)
                        {
                            currentSlide = currentSlide + 1;
                            CheckSlide();
                        }
                    }
                }
            break;
        }
    }
}



    // Set up the relative positions of each slide in the scrollbar
void SetUpScrollBar()
{
    float j;
    scrollbar.value = 0;
    blackBackground.enabled = true;
    for (int i = 0; i < mySlide.Count; i++)
    {
        j = (float)i / ((float)(mySlide.Count) - 1.0f);
        posScrollValue.Add(j);
    }
}



// Coroutine to handle the sliding animation
IEnumerator HandleSlidind(float myFinalPos, int myLocalSlideNumber)
{
    isSliding = true;
    float timeOut = 0.0f;
    blackBackground.enabled = true;
    while (scrollbar.value != myFinalPos && timeOut < 0.5f)
    {
        scrollRect.enabled = true; // Enable the ScrollRect component to allow scrolling during the slide animation
        scrollbar.value = Mathf.Lerp(scrollbar.value, myFinalPos, 0.2f); // Smoothly interpolate the scrollbar value
        ReziseCarousel(myLocalSlideNumber); // Resize the carousel slides during the animation
        timeOut += Time.deltaTime;
        yield return null;
    }
    ReziseCarousel(myLocalSlideNumber); // Resize the carousel slides at the end of the animation
    ForcePositionCarousel(myLocalSlideNumber); // Ensure the scrollbar is at the correct position
    blackBackground.enabled = false;
    scrollRect.enabled = false; // Disable the ScrollRect component again after the animation
    isSliding = false;
}



// Check if the current slide needs to be animated
void CheckSlide()
{
    for (int i = 0; i < mySlide.Count; i++)
    {
        if (currentSlide == i && isSliding == false)
        {
            StartCoroutine(HandleSlidind(posScrollValue[i], i));
        }
    }
}



// Set the scrollbar position immediately to the correct slide
void ForcePositionCarousel(int myCureentSlide)
{
    for (int i = 0; i < mySlide.Count; i++)
    {
        if (i == myCureentSlide)
        {
            scrollbar.value = posScrollValue[i];
        }
    }
}


   // Resize the carousel slides based on the current slide
void ReziseCarousel(int myCurentSlide)
{
    mySlide[myCurentSlide].transform.localScale = Vector3.Lerp(mySlide[myCurentSlide].transform.localScale, Vector3.one, 0.1f);
    // Increase the size of the current slide to make it more prominent

    for (int i = 0; i < mySlide.Count; i++)
    {
        if (i != myCurentSlide)
        {
            mySlide[i].transform.localScale = Vector3.Lerp(mySlide[i].transform.localScale, new Vector3(0.8f, 0.8f, 0.8f), 0.1f);
            // Decrease the size of other slides to make them less prominent
        }
    }
}

}