using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;

public class MainMenuTutorialController : MonoBehaviour
{
    [SerializeField] private SwipeUI _swipe;
    [SerializeField] private TutorialController _hometutorial;

    private int _currentWindow = 0;
    public bool IsTutorialInProgress => _hometutorial.IsTutorialInProgress;

    // Start is called before the first frame update
    void Awake()
    {
        if(_swipe != null)
        {
            _currentWindow = _swipe.CurrentPage;
            _swipe.OnCurrentPageChanged += ChangeWindow;
            ChangeWindow();
        }
        EmotionSelectorPopupScript.OnEmotionInsertFinished += ActivateTutorial;
    }

    private void OnDestroy()
    {
        if (_swipe != null)
        {
            _swipe.OnCurrentPageChanged -= ChangeWindow;
        }
        EmotionSelectorPopupScript.OnEmotionInsertFinished -= ActivateTutorial;
    }

    private void ChangeWindow()
    {
        if (!EmotionSelectorPopupScript.EmotionInsertedToday) return;
        var tutorial = GetTutorial(2);
        if (tutorial != null)
            tutorial.gameObject.SetActive(false);
        _currentWindow = _swipe.CurrentPage;
        tutorial = GetTutorial(2);
        if (tutorial != null)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.RefreshPositions();
        }
    }

    public void ActivateTutorial()
    {
        _hometutorial.Initialize();
        if (_hometutorial != null)
        {
            _hometutorial.gameObject.SetActive(true);
            _hometutorial.RefreshPositions();
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
