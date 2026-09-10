using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : AltMonoBehaviour
{
    [SerializeField] private TutorialStartHandler _tutorialStart;
    [SerializeField] private List<TutorialPanelHandler> _tutorialPanelList;
    [SerializeField] private string _tutorialPanelName;
    [SerializeField] private SwipeUI _swipe;
    [SerializeField] private Button _tutorialStopButton1;
    [SerializeField] private Button _tutorialStopButton2;

    private int _currentPage=0;
    private string _playerName = string.Empty;

    private bool _inProgress = false;
    public bool IsTutorialInProgress => _inProgress;

    [SerializeField] private SkipTutorialHandler _skipTutorialHandler;

    // Start is called before the first frame update
    void Start()
    {
        _tutorialStart.OnTutorialStarted += StartTutorial;
        _tutorialStart.OnTutorialSkip += SkipTutorial;

        if (_tutorialStopButton1 != null)
            _tutorialStopButton1.onClick.AddListener(() => SkipTutorial());

        if (_tutorialStopButton2 != null)
            _tutorialStopButton2.onClick.AddListener(() => SkipTutorial());
    }

    public void Initialize()
    {
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => _playerName = p.Name);
        if (PlayerPrefs.GetInt(_tutorialPanelName + "_" + _playerName, 0) == 1) return;
        if (_tutorialPanelList.Count <= 0) return;
        foreach (var tutorialPanel in _tutorialPanelList)
        {
            tutorialPanel.SetData(AdvanceTutorial);
        }
        _currentPage = -1;
        _tutorialStart.gameObject.SetActive(true);
        _inProgress = true;
    }

    public void SkipTutorial()
    {
        if(_currentPage < 0)_tutorialStart.gameObject.SetActive(false);
        else _tutorialPanelList[_currentPage].gameObject.SetActive(false);
        PlayerPrefs.SetInt(_tutorialPanelName + "_" + _playerName, 1);
        _inProgress = false;
        _tutorialStopButton1.gameObject.SetActive(false);
        _tutorialStopButton2.gameObject.SetActive(false);
    }
    private void StartTutorial()
    {
        _tutorialStart.gameObject.SetActive(false);
        _currentPage = 0;
        if (_tutorialPanelList.Count == 0) return;
        _tutorialPanelList[0].gameObject.SetActive(true);
        _skipTutorialHandler.SetSkipTutorialPanel(_currentPage);
    }

    private void AdvanceTutorial()
    {
        _tutorialPanelList[_currentPage].gameObject.SetActive(false);
        _currentPage++;
        _skipTutorialHandler.SetSkipTutorialPanel(_currentPage);
        if (_tutorialPanelList.Count > _currentPage)
        {
            _tutorialPanelList[_currentPage].gameObject.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt(_tutorialPanelName + "_" + _playerName, 1);
            _inProgress = false;
            _tutorialStopButton1.gameObject.SetActive(false);
            _tutorialStopButton2.gameObject.SetActive(false);
        }
    }

    public void RefreshPositions()
    {
        foreach(var tutorialPanel in _tutorialPanelList)
        {
            tutorialPanel.UpdatePosition();
        }
    }

}
