using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

public class NextSlideButton : MonoBehaviour
{
    public SwipeUI swipeUI;
    public IntroSwipeUI introSwipeUI;

    private void Start()
    {
        // Attach a click event handler to the button
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Call the GoToNextSlide method in the SwipeUI script
        Debug.Log("button currentpage: " + swipeUI.CurrentPage + " length: " + introSwipeUI.startSlides.Length);

        if (swipeUI.CurrentPage >= introSwipeUI.startSlides.Length - 1)
        {
            return;
        }
        else
        {
            swipeUI.NextSlide();
        }
        return;
    }
}
