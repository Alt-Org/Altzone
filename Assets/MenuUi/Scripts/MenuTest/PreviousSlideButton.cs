using System.Collections;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class PreviousSlideButton : MonoBehaviour
{
    public ManagerCarousel carouselManager;

    private void Start()
    {
        // Attach a click event handler to the button
        Button button = GetComponent<Button>();
        if(carouselManager != null)
        button.onClick.AddListener(OnButtonClick);
        else
        {
            button.onClick.AddListener(() =>
            {
                // Better have one frame delay to let other button listeners execute before actually closing current window and going back
                StartCoroutine(GoBack());
            });
        }
    }

    private void OnButtonClick()
    {
        // Call the GoToNextSlide method in the ManagerCarousel script
        carouselManager.GoToPreviousSlide();
    }

    private static IEnumerator GoBack()
    {
        yield return null;
        WindowManager.Get().GoBack();
    }
}
