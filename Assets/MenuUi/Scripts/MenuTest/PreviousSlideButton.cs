using UnityEngine;
using UnityEngine.UI;

public class PreviousSlideButton : MonoBehaviour
{
    public ManagerCarousel carouselManager;

    private void Start()
    {
        // Attach a click event handler to the button
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Call the GoToNextSlide method in the ManagerCarousel script
        carouselManager.GoToPreviousSlide();
    }
}
