using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;

public class MainMenuTutorialController : MonoBehaviour
{
    [SerializeField] private SwipeUI _swipe;
    [SerializeField] private TutorialController _hometutorial;

    private int _currentWindow = 0;

    // Start is called before the first frame update
    void Start()
    {
        _currentWindow = _swipe.CurrentPage;
        _swipe.OnCurrentPageChanged += ChangeWindow;
        EmotionSelectorPopupScript.OnEmotionInsertFinished += ChangeWindow;
        ChangeWindow();
    }

    private void OnDestroy()
    {
        EmotionSelectorPopupScript.OnEmotionInsertFinished -= ChangeWindow;
    }

    private void ChangeWindow()
    {
        if (!EmotionSelectorPopupScript.EmotionInsertedToday) return;
        var tutorial = GetTutorial(_currentWindow);
        if (tutorial != null)
            tutorial.gameObject.SetActive(false);
        _currentWindow = _swipe.CurrentPage;
        tutorial = GetTutorial(_currentWindow);
        if (tutorial != null)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.RefreshPositions();
        }
    }

    private TutorialController GetTutorial(int page)
    {
        return page switch
        {
            2 => _hometutorial,
            _ => null,
        };
    }
}
