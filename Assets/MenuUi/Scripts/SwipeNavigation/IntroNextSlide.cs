using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

public class IntroNextSlide : MonoBehaviour
{
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

        introSwipeUI.NextSlide();
    }
}